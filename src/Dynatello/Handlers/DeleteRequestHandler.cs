using Amazon.DynamoDBv2.Model;
using Dynatello.Pipelines;

namespace Dynatello.Handlers;

internal sealed class DeleteRequestHandler<TArg, T> : IRequestHandler<TArg, T?>
{
    private readonly HandlerOptions _handlerOptions;
    private readonly Func<TArg, DeleteItemRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _createItem;

    internal DeleteRequestHandler(
        HandlerOptions handlerOptions,
        Func<TArg, DeleteItemRequest> createRequest,
        Func<Dictionary<string, AttributeValue>, T> createItem
    )
    {
        _handlerOptions = handlerOptions;
        _createRequest = createRequest;
        _createItem = createItem;
    }

    public async Task<T?> Send(TArg arg, CancellationToken cancellationToken)
    {
        var request = _createRequest(arg);
        var response = await request.SendRequest(
            _handlerOptions.RequestsPipelines,
            (x, y, z) => y.DeleteItemAsync(x, z),
            _handlerOptions.AmazonDynamoDB,
            cancellationToken
        );

        return request.ReturnValues.IsValueProvided() ? _createItem(response.Attributes) : default;
    }
}
