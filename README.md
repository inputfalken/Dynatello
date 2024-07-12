# Dynatello

## What does it do?
A reflection free & generic typed solution to send requests towards dynamodb without having to manually maintain the low-level parts.

## Features

* Builder patterns to create request builders.
* Request handlers that perform the DynamoDB request and handles the response.
    * TODO add `response` middlewares.
    * TODO add optional error handlers.

## Installation

Add the following NuGet package as a dependency to you project.

[![DynamoDBGenerator][1]][2]

[1]: https://img.shields.io/nuget/v/Dynatello.svg?label=Dynatello
[2]: https://www.nuget.org/packages/Dynatello

## Example

```csharp
using System.Diagnostics;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using Dynatello.Handlers;
using Dynatello.Pipelines;

ProductRepository productRepository = new ProductRepository("PRODUCTS", new AmazonDynamoDBClient());

public class ProductRepository
{
    private readonly IRequestHandler<string, Product?> _getById;
    private readonly IRequestHandler< (string Id, decimal NewPrice, DateTime TimeStamp), Product?> _updatePrice;
    private readonly IRequestHandler<Product, Product?> _createProduct;
    private readonly IRequestHandler<decimal, IReadOnlyList<Product>> _queryByPrice;
    private readonly IRequestHandler<string, Product?> _deleteById;

    private class RequestLogger : IRequestPipeLine
    {
        public async Task<AmazonWebServiceResponse> Invoke(
            RequestContext requestContext,
            Func<RequestContext, Task<AmazonWebServiceResponse>> continuation
        )
        {
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Starting request");
            var request = await continuation(requestContext);
            Console.WriteLine($"Request finished after '{stopwatch.Elapsed}'.");
            return request;
        }
    }

    public ProductRepository(string tableName, IAmazonDynamoDB amazonDynamoDb)
    {
        var requestLogger = new RequestLogger();
        _getById = Product
            .WithIdArgument.OnTable(tableName)
            .ToGetRequestHandler(
                x => x.ToGetRequestBuilder(),
                x =>
                {
                    x.AmazonDynamoDB = amazonDynamoDb;
                    // All handlers comes with the ability to add middlewares that will be executed.
                    x.RequestsPipelines.Add(requestLogger);
                }
            );

        _deleteById = Product
            .WithIdArgument.OnTable(tableName)
            .ToDeleteRequestHandler(
                x => x.ToDeleteRequestBuilder(),
                x => x.AmazonDynamoDB = amazonDynamoDb
            );

        _updatePrice = Product
            .UpdatePrice.OnTable(tableName)
            .ToUpdateRequestHandler(
                x =>
                    x.WithUpdateExpression(
                            (db, arg) =>
                                $"SET {db.Price} = {arg.NewPrice}, {db.Metadata.ModifiedAt} = {arg.TimeStamp}"
                        ) // Specify the update operation
                        .ToUpdateItemRequestBuilder(
                            ((marshaller, arg) => marshaller.PartitionKey(arg.Id))
                        ),
                x => x.AmazonDynamoDB = amazonDynamoDb
            );

        _createProduct = Product
            .Put.OnTable(tableName)
            .ToPutRequestHandler(
                x =>
                    x.WithConditionExpression((db, arg) => $"{db.Id} <> {arg.Id}") // Ensure we don't have an existing Product in DynamoDB
                        .ToPutRequestBuilder(),
                x => x.AmazonDynamoDB = amazonDynamoDb
            );

        _queryByPrice = Product
            .QueryByPrice.OnTable(tableName)
            .ToQueryRequestHandler(
                x =>
                    x.WithKeyConditionExpression((db, arg) => $"{db.Price} = {arg}")
                        .ToQueryRequestBuilder() with
                    {
                        IndexName = Product.PriceIndex
                    },
                x => x.AmazonDynamoDB = amazonDynamoDb
            );

        // You can also use a RequestBuilder if you want to handle the response yourself.
        GetRequestBuilder<string> getProductByIdRequestBuilder = Product
            .WithIdArgument.OnTable(tableName)
            .ToRequestBuilderFactory()
            .ToGetRequestBuilder();
    }

    public Task<IReadOnlyList<Product>> SearchByPrice(decimal price) =>
        _queryByPrice.Send(price, default);

    public Task<Product?> Create(Product product) => _createProduct.Send(product, default);

    public Task<Product?> GetById(string id) => _getById.Send(id, default);

    public Task<Product?> DeleteById(string id) => _deleteById.Send(id, default);

    public Task<Product?> UpdatePrice(string id, decimal price) =>
        _updatePrice.Send((id, price, DateTime.UtcNow), default);
}

// These attributes is what makes the source generator kick in. Make sure to have the class 'partial' as well.
[DynamoDBMarshaller(AccessName = "Put")]
[DynamoDBMarshaller(AccessName = "WithIdArgument", ArgumentType = typeof(string))]
[DynamoDBMarshaller(AccessName = "UpdatePrice", ArgumentType = typeof((string Id, decimal NewPrice, DateTime TimeStamp)))]
[DynamoDBMarshaller(AccessName = "QueryByPrice", ArgumentType = typeof(decimal))]
public partial record Product(
    [property: DynamoDBHashKey, DynamoDBGlobalSecondaryIndexRangeKey(Product.PriceIndex)] string Id,
    [property: DynamoDBGlobalSecondaryIndexHashKey(Product.PriceIndex)] decimal Price,
    string Description,
    Product.MetadataEntity Metadata
)
{
    public const string PriceIndex = "PriceIndex";

    public record MetadataEntity(DateTime CreatedAt, DateTime ModifiedAt);
}
```
