using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Dynatello.Pipelines;

namespace Dynatello.Handlers;

internal sealed class QueryRequestHandler<TArg, T> : IRequestHandler<TArg, IReadOnlyList<T>>
{
    private readonly IAmazonDynamoDB _client;
    private readonly IList<IRequestPipeLine> _pipelines;
    private readonly Func<TArg, QueryRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _createItem;

    internal QueryRequestHandler(HandlerOptions options, Func<TArg, QueryRequest> createRequest, Func<Dictionary<string, AttributeValue>, T> createItem)
    {
        _client = options.AmazonDynamoDB;
        _pipelines = options.RequestsPipelines;
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
            response = await request.SendRequest<QueryRequest, QueryResponse>(_pipelines, (x,y,z) => y.QueryAsync(x,z), _client, cancellationToken);
            request.ExclusiveStartKey = response.LastEvaluatedKey;

            list.AddRange(response.Items.Select(_createItem));

        } while (response.LastEvaluatedKey.Count > 0);

        return list;
    }
}
