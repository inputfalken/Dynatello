using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;

namespace Dynatello.Handlers;
/// <summary>
/// A request handler for sending a <see cref="GetItemRequest"/> and recieving a <see cref="GetItemResponse"/> whose payload will be unmarshlled into <typeparamref name="T"/>
/// </summary>
public record struct GetRequestHandler<T, TArg> : IRequestHandler<T, TArg>, IRequestMiddleware<GetItemRequest>, IResponseMiddleware<GetItemResponse>
  where T : notnull
  where TArg : notnull
{
    private readonly IAmazonDynamoDB _client;
    private readonly Func<TArg, GetItemRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _createItem;
    private Action<GetItemResponse>? _onResponse;
    private Action<GetItemRequest>? _onRequest;

    internal GetRequestHandler(IAmazonDynamoDB client, Func<TArg, GetItemRequest> createRequest, Func<Dictionary<string, AttributeValue>, T> createItem)
    {
        _client = client;
        _createRequest = createRequest;
        _createItem = createItem;
        _onResponse = null;
        _onRequest = null;
    }

    /// <summary>
    /// 
    /// </summary>
    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public GetRequestHandler()
    {
        throw Constants.InvalidConstructor();
    }

    /// <summary>
    /// Sends a request towards DynamoDB and unmarshalls the response.
    /// </summary>
    /// <exception cref="DynamoDBMarshallingException">
    /// If the unmarshalling could not build the <typeparamref name="T"/> correctly due to missing required values.
    /// </exception>
    /// <remarks>
    /// Will not do any exception handling when invcoking <see cref="IAmazonDynamoDB.GetItemAsync(GetItemRequest, CancellationToken)"/>.
    /// </remarks>
    public async Task<T?> Send(TArg arg, CancellationToken cancellationToken)
    {
        var request = _createRequest(arg);
        _onRequest?.Invoke(request);

        var response = await _client.GetItemAsync(request, cancellationToken);
        _onResponse?.Invoke(response);

        return response.IsItemSet
          ? _createItem(response.Item)
          : default;
    }

    public void Configure(Action<GetItemRequest> configure) => _onRequest = configure;

    public void Configure(Action<GetItemResponse> configure) => _onResponse = configure;
}
