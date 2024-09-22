# Dynatello

## What does the library do?

Offers a unified API based on the source generated low-level API provided from [DynamoDB.SourceGenerator](https://github.com/inputfalken/DynamoDB.SourceGenerator). 

## Features

* Builder patterns to create request builders.
* Request handlers that perform the DynamoDB request and handles the response.
  *  Middlware Support through `IRequestPipeline`. 

## Installation

Add the following NuGet package as a dependency to your project.

[![DynamoDBGenerator][1]][2]

[1]: https://img.shields.io/nuget/v/Dynatello.svg?label=Dynatello
[2]: https://www.nuget.org/packages/Dynatello

## Example
The code is also available in the [samples](samples/) directory.

### Extend DTO's
Extend a DTO to contain the mashaller functionality.

```csharp
using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Handlers;

IRequestHandler<string, Cat?> getById = Cat
    .FromId.OnTable("TABLE")
    .ToGetRequestHandler(x => x.ToGetRequestBuilder());

if (args.Length == 1)
{
    Cat? response = await getById.Send(args[0], CancellationToken.None);

    Console.WriteLine(response);
}
else
{
    throw new NotImplementedException();
}

[DynamoDBMarshaller(AccessName = "FromId", ArgumentType = typeof(string))]
public partial record Cat(string Id, string Name, double Cuteness);
```


### Request middleware

Every `RequestHandler<Targ, TResponse>` supports having multiple middlewares.

```csharp
using System.Diagnostics;
using Amazon.Runtime;
using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Handlers;
using Dynatello.Pipelines;

IRequestHandler<string, Cat?> getById = Cat
    .FromId.OnTable("TABLE")
    .ToGetRequestHandler(
        x => x.ToGetRequestBuilder(),
        x =>
        {
            x.RequestsPipelines.Add(new RequestDurationConsoleLogger());
        }
    );

if (args.Length == 1)
{
    Cat? response = await getById.Send(args[0], CancellationToken.None);

    Console.WriteLine(response);
}
else
{
    throw new NotImplementedException();
}

public class RequestDurationConsoleLogger : IRequestPipeLine
{
    public async Task<AmazonWebServiceResponse> Invoke(
        RequestContext requestContext,
        Func<RequestContext, Task<AmazonWebServiceResponse>> continuation
    )
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await continuation(requestContext);
        Console.WriteLine($"Duration: {stopwatch.Elapsed}");
        
        return result;
    }
}

[DynamoDBMarshaller(AccessName = "FromId", ArgumentType = typeof(string))]
public partial record Cat(string Id, string Name, double Cuteness);
```


### Repository
Isolate DynamoDB code to a repository class.

```csharp
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using Dynatello.Handlers;

ProductRepository productRepository = new ProductRepository("PRODUCTS", new AmazonDynamoDBClient());

// These attributes is what makes the source generator kick in. Make sure to have the class 'partial' as well.
[DynamoDBMarshaller(EntityType = typeof(Product), AccessName = "FromProduct")]
[DynamoDBMarshaller(
    EntityType = typeof(Product),
    AccessName = "FromId",
    ArgumentType = typeof(string)
)]
[DynamoDBMarshaller(
    EntityType = typeof(Product),
    AccessName = "FromUpdatePricePayload",
    ArgumentType = typeof((string Id, decimal NewPrice, DateTime TimeStamp))
)]
[DynamoDBMarshaller(
    EntityType = typeof(Product),
    AccessName = "FromPrice",
    ArgumentType = typeof(decimal)
)]
public partial class ProductRepository
{
    private readonly IRequestHandler<string, Product?> _getById;
    private readonly IRequestHandler<
        (string Id, decimal NewPrice, DateTime TimeStamp),
        Product?
    > _updatePrice;
    private readonly IRequestHandler<Product, Product?> _createProduct;
    private readonly IRequestHandler<decimal, IReadOnlyList<Product>> _queryByPrice;
    private readonly IRequestHandler<string, Product?> _deleteById;

    public ProductRepository(string tableName, IAmazonDynamoDB amazonDynamoDb)
    {
        _getById = FromId
            .OnTable(tableName)
            .ToGetRequestHandler(
                x => x.ToGetRequestBuilder(),
                x =>
                {
                    x.AmazonDynamoDB = amazonDynamoDb;
                }
            );

        _deleteById = FromId
            .OnTable(tableName)
            .ToDeleteRequestHandler(
                x => x.ToDeleteRequestBuilder(),
                x => x.AmazonDynamoDB = amazonDynamoDb
            );

        _updatePrice = FromUpdatePricePayload
            .OnTable(tableName)
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

        _createProduct = FromProduct
            .OnTable(tableName)
            .ToPutRequestHandler(
                x =>
                    x.WithConditionExpression((db, arg) => $"{db.Id} <> {arg.Id}") // Ensure we don't have an existing Product in DynamoDB
                        .ToPutRequestBuilder(),
                x => x.AmazonDynamoDB = amazonDynamoDb
            );

        _queryByPrice = FromPrice
            .OnTable(tableName)
            .ToQueryRequestHandler(
                x =>
                    x.WithKeyConditionExpression((db, arg) => $"{db.Price} = {arg}")
                        .ToQueryRequestBuilder() with
                    {
                        IndexName = Product.PriceIndex
                    },
                x => x.AmazonDynamoDB = amazonDynamoDb
            );
    }

    public Task<IReadOnlyList<Product>> SearchByPrice(decimal price) =>
        _queryByPrice.Send(price, default);

    public Task<Product?> Create(Product product) => _createProduct.Send(product, default);

    public Task<Product?> GetById(string id) => _getById.Send(id, default);

    public Task<Product?> DeleteById(string id) => _deleteById.Send(id, default);

    public Task<Product?> UpdatePrice(string id, decimal price) =>
        _updatePrice.Send((id, price, DateTime.UtcNow), default);
}

public record Product(
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
