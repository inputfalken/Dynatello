using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Exceptions;
using Dynatello.Pipelines;

namespace Dynatello.Handlers;
/// <summary>
/// A request handler for sending a <see cref="GetItemRequest"/> and recieving a <see cref="GetItemResponse"/> whose payload will be unmarshlled into <typeparamref name="T"/>
/// </summary>
internal sealed class GetRequestHandler<TArg, T> : IRequestHandler<TArg, T?>
  where T : notnull
  where TArg : notnull
{
    private readonly IAmazonDynamoDB _client;
    private readonly Func<TArg, GetItemRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _createItem;
    private readonly IEnumerable<IRequestPipeLine> _requestsPipelines;

    internal GetRequestHandler(
        IAmazonDynamoDB client,
        Func<TArg, GetItemRequest> createRequest,
        Func<Dictionary<string, AttributeValue>, T> createItem,
        IEnumerable<IRequestPipeLine> requestsPipelines
    )
    {
        _client = client;
        _createRequest = createRequest;
        _createItem = createItem;
        _requestsPipelines = requestsPipelines;
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
        var response = await _createRequest(arg)
          .SendRequest<GetItemRequest, GetItemResponse>(_requestsPipelines, async (x, y, z) => await y.GetItemAsync(x, z), _client, cancellationToken);

        return response.IsItemSet
          ? _createItem(response.Item)
          : default;
    }
}
