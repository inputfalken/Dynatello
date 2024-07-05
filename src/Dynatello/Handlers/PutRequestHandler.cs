using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Dynatello.Pipelines;

namespace Dynatello.Handlers;

internal sealed class PutRequestHandler<T> : IRequestHandler<T, T?>
  where T : notnull
{
    private readonly IAmazonDynamoDB _client;
    private readonly IList<IRequestPipeLine> _pipelines;
    private readonly Func<T, PutItemRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _createItem;

    internal PutRequestHandler(HandlerOptions options, Func<T, PutItemRequest> createRequest, Func<Dictionary<string, AttributeValue>, T> createItem)
    {
        _client = options.AmazonDynamoDB;
        _pipelines = options.RequestsPipelines;
        _createRequest = createRequest;
        _createItem = createItem;
    }

    ///<inheritdoc/>
    public async Task<T?> Send(T arg, CancellationToken cancellationToken)
    {
        var request = _createRequest(arg);

        var response = await request
          .SendRequest<PutItemRequest, PutItemResponse>(_pipelines, (x, y, z) => y.PutItemAsync(x, z), _client, cancellationToken);

        return request.ReturnValues.IsValueProvided()
          ? _createItem(response.Attributes)
          : default;
    }
}
