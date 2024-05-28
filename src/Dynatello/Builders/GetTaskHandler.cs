
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Dynatello.Builders;

public record struct GetTaskHandler<T, TArg> where TArg : notnull
{
    private readonly IAmazonDynamoDB _client;
    private readonly Func<TArg, GetItemRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _createItem;
    private Action<GetItemResponse>? _onResponse;
    private Action<GetItemRequest>? _onRequest;

    internal GetTaskHandler(IAmazonDynamoDB client, Func<TArg, GetItemRequest> createRequest, Func<Dictionary<string, AttributeValue>, T> createItem)
    {
        _client = client;
        _createRequest = createRequest;
        _createItem = createItem;
        _onResponse = null;
        _onRequest = null;
    }

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public GetTaskHandler()
    {
        throw Constants.InvalidConstructor();
    }

    public GetTaskHandler<T, TArg> OnResponse(Action<GetItemResponse> configure)
    {
        _onResponse = configure;
        return this;
    }

    public GetTaskHandler<T, TArg> OnRequest(Action<GetItemRequest> configure)
    {
        _onRequest = configure;
        return this;
    }

    public async Task<T> Send(TArg arg, CancellationToken cancellationToken)
    {
        var request = _createRequest(arg);
        _onRequest?.Invoke(request);
        
        var response = await _client.GetItemAsync(request, cancellationToken);
        _onResponse?.Invoke(response);

        return _createItem(response.Item);
    }


}
