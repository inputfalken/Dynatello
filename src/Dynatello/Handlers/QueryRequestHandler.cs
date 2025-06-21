using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Dynatello.Pipelines;

namespace Dynatello.Handlers;

internal sealed class QueryRequestHandler<TArg, T> : IRequestHandler<TArg, IReadOnlyList<T>>
{
    private readonly HandlerOptions _options;
    private readonly Func<TArg, QueryRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _createItem;

    internal QueryRequestHandler(
        HandlerOptions options,
        Func<TArg, QueryRequest> createRequest,
        Func<Dictionary<string, AttributeValue>, T> createItem
    )
    {
        _options = options;
        _createRequest = createRequest;
        _createItem = createItem;
    }

    public async Task<IReadOnlyList<T>> Send(TArg arg, CancellationToken cancellationToken)
    {
        var request = _createRequest(arg);
        var list = new List<T>();
        while (true)
        {
            var response = await request.SendRequest(
                _options.RequestsPipelines,
                (x, y, z) => y.QueryAsync(x, z),
                _options.AmazonDynamoDB,
                cancellationToken
            );

            if (response.Items is { Count: > 0 })
                list.AddRange(response.Items.Select(_createItem));
            
            if (response.LastEvaluatedKey is null or { Count: 0 })
                break;

            request.ExclusiveStartKey = response.LastEvaluatedKey;
        }

        return list;
    }
}