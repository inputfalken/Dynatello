using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Dynatello.Handlers;

internal sealed class PutRequestHandler<T> : IRequestHandler<T, T>
  where T : notnull
{
    private readonly IAmazonDynamoDB _client;
    private readonly Func<T, PutItemRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _createItem;

    internal PutRequestHandler(IAmazonDynamoDB client, Func<T, PutItemRequest> createRequest, Func<Dictionary<string, AttributeValue>, T> createItem)
    {
        _client = client;
        _createRequest = createRequest;
        _createItem = createItem;
    }


    ///<inheritdoc/>
    public async Task<T?> Send(T arg, CancellationToken cancellationToken)
    {
        var request = _createRequest(arg);

        var response = await _client.PutItemAsync(request, cancellationToken);

        return request.ReturnValues.IsValueProvided()
          ? _createItem(response.Attributes)
          : default;
    }
}
