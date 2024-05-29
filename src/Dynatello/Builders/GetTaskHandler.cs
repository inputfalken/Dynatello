
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;

namespace Dynatello.Builders;

/// <summary>
/// A task handler for sending a <see cref="GetItemRequest"/> and recieving a <see cref="GetItemResponse"/> whose payload will be unmarshlled into <typeparamref name="T"/>
/// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public GetTaskHandler()
    {
        throw Constants.InvalidConstructor();
    }

    /// <summary>
    /// Configure the <see cref="GetItemResponse"/> before the handler uses the response.
    /// </summary>
    public GetTaskHandler<T, TArg> OnResponse(Action<GetItemResponse> configure)
    {
        _onResponse = configure;
        return this;
    }

    /// <summary>
    /// Configure the <see cref="GetItemRequest"/> before the request is sent to DynamoDB.
    /// </summary>
    public GetTaskHandler<T, TArg> OnRequest(Action<GetItemRequest> configure)
    {
        _onRequest = configure;
        return this;
    }

    /// <summary>
    /// Sends a request towards DynamoDB and unmarshalls the response.
    /// </summary>
    /// <exception cref="DynamoDBMarshallingException">
    /// If the unmarshalling could not build the <typeparamref name="T"/> correctly due to missing required values.
    /// </exception>
    /// <remarks>
    /// Will not do any exception when invcoking <see cref="IAmazonDynamoDB.GetItemAsync(GetItemRequest, CancellationToken)"/>.
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
}
