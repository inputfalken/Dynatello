using Amazon.DynamoDBv2.Model;
using Dynatello.Pipelines;

namespace Dynatello.Handlers;

internal sealed class UpdateRequestHandler<TArg, T> : IRequestHandler<TArg, T?>
    where T : notnull
{
    private readonly HandlerOptions _options;
    private readonly Func<TArg, UpdateItemRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _unmarshall;

    internal UpdateRequestHandler(
        HandlerOptions options,
        Func<TArg, UpdateItemRequest> createRequest,
        Func<Dictionary<string, AttributeValue>, T> unmarshall
    )
    {
        _options = options;
        _createRequest = createRequest;
        _unmarshall = unmarshall;
    }

    public async Task<T?> Send(TArg arg, CancellationToken cancellationToken)
    {
        var request = _createRequest(arg);
        var response = await request.SendRequest(
            _options.RequestsPipelines,
            (x, y, z) => y.UpdateItemAsync(x, z),
            _options.AmazonDynamoDB,
            cancellationToken
        );

        return request.ReturnValues.IsValueProvided() ? _unmarshall(response.Attributes) : default;
    }
}