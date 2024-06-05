using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Dynatello.Handlers;

internal sealed class UpdateRequestHandler<T> : IRequestHandler<T>
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly Func<T, UpdateItemRequest> _createRequest;
    private readonly Func<AttributeValue, T> _unmarshall;

    internal UpdateRequestHandler(IAmazonDynamoDB dynamoDb, Func<T, UpdateItemRequest> createRequest)
    {
        _dynamoDb = dynamoDb;
        _createRequest = createRequest;
    }

    public Task Send(T arg, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

