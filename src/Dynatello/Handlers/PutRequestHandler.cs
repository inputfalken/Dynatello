using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Dynatello.Handlers;

public record struct PutRequestHandler<T> : IRequestHandler<T, T>, IRequestMiddleware<PutItemRequest>, IResponseMiddleware<PutItemResponse>
  where T : notnull
{
    private readonly IAmazonDynamoDB _client;
    private readonly Func<T, PutItemRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _createItem;
    private Action<PutItemResponse>? _onResponse;
    private Action<PutItemRequest>? _onRequest;

    internal PutRequestHandler(IAmazonDynamoDB client, Func<T, PutItemRequest> createRequest, Func<Dictionary<string, AttributeValue>, T> createItem)
    {
        _client = client;
        _createRequest = createRequest;
        _createItem = createItem;
        _onResponse = null;
        _onRequest = null;
    }

    ///<inheritdoc/>
    public void Configure(Action<PutItemRequest> configure) => _onRequest = configure;

    ///<inheritdoc/>
    public void Configure(Action<PutItemResponse> configure) => _onResponse = configure;
    
    ///<inheritdoc/>
    public async Task<T?> Send(T arg, CancellationToken cancellationToken)
    {
        var request = _createRequest(arg);
        _onRequest?.Invoke(request);

        var response = await _client.PutItemAsync(request, cancellationToken);
        _onResponse?.Invoke(response);

        return request.ReturnValues.IsValueProvided()
          ? _createItem(response.Attributes)
          : default;
    }
}
