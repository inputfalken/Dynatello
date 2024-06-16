using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Dynatello.Handlers;

internal sealed class UpdateRequestHandler<TArg, T> : IRequestHandler<TArg, T?>
where T : notnull
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly Func<TArg, UpdateItemRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _unmarshall;

    internal UpdateRequestHandler(IAmazonDynamoDB dynamoDb, Func<TArg, UpdateItemRequest> createRequest, Func<Dictionary<string, AttributeValue>, T> unmarshall)
    {
        _dynamoDb = dynamoDb;
        _createRequest = createRequest;
        _unmarshall = unmarshall;
    }


    public async Task<T?> Send(TArg arg, CancellationToken cancellationToken)
    {
        var request = _createRequest(arg);
        var response = await _dynamoDb.UpdateItemAsync(request, cancellationToken);

        return request.ReturnValues.IsValueProvided()
            ? _unmarshall(response.Attributes)
            : default;
    }
}

