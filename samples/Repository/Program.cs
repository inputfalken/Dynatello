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
