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
        HandlerOptions handlerOptions,
        Func<TArg, GetItemRequest> createRequest,
        Func<Dictionary<string, AttributeValue>, T> createItem
    )
    {
        _client = handlerOptions.AmazonDynamoDB;
        _createRequest = createRequest;
        _createItem = createItem;
        _requestsPipelines = handlerOptions.RequestsPipelines;
    }

    /// <summary>
    /// Sends a request towards DynamoDB and unmarshalls the response.
    /// </summary>
    /// <exception cref="DynamoDBMarshallingException">
    /// If the unmarshalling could not build the <typeparamref name="T"/> correctly due to missing required values.
    /// </exception>
    /// <remarks>
    /// Will not do any exception handling when invoking <see cref="IAmazonDynamoDB.GetItemAsync(GetItemRequest, CancellationToken)"/>.
    /// </remarks>
    public async Task<T?> Send(TArg arg, CancellationToken cancellationToken)
    {
        var response = await _createRequest(arg)
          .SendRequest<GetItemRequest, GetItemResponse>(
            _requestsPipelines,
            (x, y, z) => y.GetItemAsync(x, z),
            _client,
            cancellationToken
          );

        return response.IsItemSet
          ? _createItem(response.Item)
          : default;
    }
}
