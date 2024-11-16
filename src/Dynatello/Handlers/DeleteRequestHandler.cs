using Amazon.DynamoDBv2.Model;
using Dynatello.Pipelines;

namespace Dynatello.Handlers;

internal sealed class DeleteRequestHandler<TArg, T> : IRequestHandler<TArg, T?>
{
    private readonly HandlerOptions _options;
    private readonly Func<TArg, DeleteItemRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _createItem;

    internal DeleteRequestHandler(
        HandlerOptions options,
        Func<TArg, DeleteItemRequest> createRequest,
        Func<Dictionary<string, AttributeValue>, T> createItem
    )
    {
        _options = options;
        _createRequest = createRequest;
        _createItem = createItem;
    }

    public async Task<T?> Send(TArg arg, CancellationToken cancellationToken)
    {
        var request = _createRequest(arg);
        var response = await request.SendRequest(
            _options.RequestsPipelines,
            (x, y, z) => y.DeleteItemAsync(x, z),
            _options.AmazonDynamoDB,
            cancellationToken
        );

        return request.ReturnValues.IsValueProvided() ? _createItem(response.Attributes) : default;
    }
}