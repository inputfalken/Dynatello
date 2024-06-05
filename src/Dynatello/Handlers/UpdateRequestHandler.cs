using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Dynatello.Handlers;

internal sealed class UpdateRequestHandler<T> : IRequestHandler<UpdateItemResponse, T>
where T : notnull
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly Func<T, UpdateItemRequest> _createRequest;

    internal UpdateRequestHandler(IAmazonDynamoDB dynamoDb, Func<T, UpdateItemRequest> createRequest)
    {
        _dynamoDb = dynamoDb;
        _createRequest = createRequest;
    }

    public async Task<UpdateItemResponse> Send(T arg, CancellationToken cancellationToken)
    {
        var request = _createRequest(arg);
        var response = await _dynamoDb.UpdateItemAsync(request, cancellationToken);

        return response;
    }
}

