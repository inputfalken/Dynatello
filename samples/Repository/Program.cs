﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Handlers;
using Dynatello.Builders.Types;

ProductRepository productRepository = new ProductRepository("PRODUCTS", new AmazonDynamoDBClient());

public class ProductRepository
{
    private readonly IRequestHandler<string, Product?> _getProductByIdRequest;
    private readonly IRequestHandler<(string Id, decimal NewPrice, DateTime TimeStamp), Product?> _updatePrice;
    private readonly IRequestHandler<Product, Product?> _createProduct;
    private readonly IRequestHandler<decimal, IReadOnlyList<Product>> _queryByPrice;

    public ProductRepository(string tableName, IAmazonDynamoDB amazonDynamoDb)
    {
        _getProductByIdRequest = Product.GetById
            .OnTable(tableName)
            .ToGetRequestHandler(x => x.ToGetRequestBuilder(), amazonDynamoDb);

        _updatePrice = Product.UpdatePrice
            .OnTable(tableName)
            .ToUpdateRequestHandler(
              x => x
                  .WithUpdateExpression((db, arg) => $"SET {db.Price} = {arg.NewPrice}, {db.Metadata.ModifiedAt} = {arg.TimeStamp}") // Specify the update operation
                  .ToUpdateItemRequestBuilder(((marshaller, arg) => marshaller.PartitionKey(arg.Id))),
              amazonDynamoDb
            );

        _createProduct = Product.Put
            .OnTable(tableName)
            .ToPutRequestHandler(
              x => x
                .WithConditionExpression((db, arg) => $"{db.Id} <> {arg.Id}") // Ensure we don't have an existing Product in DynamoDB
                .ToPutRequestBuilder(),
              amazonDynamoDb
            );

        _queryByPrice = Product.QueryByPrice
                .OnTable(tableName)
                .ToQueryRequestHandler(
                  x => x
                    .WithKeyConditionExpression((db, arg) => $"{db.Price} = {arg}")
                    .ToQueryRequestBuilder() with
                  { IndexName = Product.PriceIndex },
                  amazonDynamoDb
                );
        
        // You can also use a RequestBuilder if you want to handle the response yourself.
        GetRequestBuilder<string> getProductByIdRequestBuilder = Product.GetById
          .OnTable(tableName)
          .ToRequestBuilderFactory()
          .ToGetRequestBuilder();
    }

    public Task<IReadOnlyList<Product>> SearchByPrice(decimal price) => _queryByPrice.Send(price, default);

    public Task<Product?> Create(Product product) => _createProduct.Send(product, default);

    public Task<Product?> GetById(string id) => _getProductByIdRequest.Send(id, default);

    public Task<Product?> UpdatePrice(string id, decimal price) => _updatePrice.Send((id, price, DateTime.UtcNow), default);
}

// These attributes is what makes the source generator kick in. Make sure to have the class 'partial' as well.
[DynamoDBMarshaller(AccessName = "Put")]
[DynamoDBMarshaller(AccessName = "GetById", ArgumentType = typeof(string))]
[DynamoDBMarshaller(AccessName = "UpdatePrice", ArgumentType = typeof((string Id, decimal NewPrice, DateTime TimeStamp)))]
[DynamoDBMarshaller(AccessName = "QueryByPrice", ArgumentType = typeof(decimal))]
public partial record Product(
    [property: DynamoDBHashKey, DynamoDBGlobalSecondaryIndexRangeKey(Product.PriceIndex)] string Id,
    [property: DynamoDBGlobalSecondaryIndexHashKey(Product.PriceIndex)] decimal Price,
    string Description,
    Product.MetadataEntity Metadata)
{
    public const string PriceIndex = "PriceIndex";

    public record MetadataEntity(DateTime CreatedAt, DateTime ModifiedAt);
}
