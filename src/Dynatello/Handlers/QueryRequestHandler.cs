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
        QueryResponse response;

        var list = new List<T>();
        do
        {
            response = await request.SendRequest(
                _options.RequestsPipelines,
                (x, y, z) => y.QueryAsync(x, z),
                _options.AmazonDynamoDB,
                cancellationToken
            );
            request.ExclusiveStartKey = response.LastEvaluatedKey;

            list.AddRange(response.Items.Select(_createItem));
        } while (response.LastEvaluatedKey.Count > 0);

        return list;
    }
}
