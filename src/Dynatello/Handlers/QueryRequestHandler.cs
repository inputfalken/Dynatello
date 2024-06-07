using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Dynatello.Handlers;

internal sealed class QueryRequestHandler<T, TArg> : IRequestHandler<IReadOnlyList<T>, TArg>
{
    private readonly IAmazonDynamoDB _client;
    private readonly Func<TArg, QueryRequest> _createRequest;
    private readonly Func<Dictionary<string, AttributeValue>, T> _createItem;

    internal QueryRequestHandler(IAmazonDynamoDB client, Func<TArg, QueryRequest> createRequest, Func<Dictionary<string, AttributeValue>, T> createItem)
    {
        _client = client;
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
            response = await _client.QueryAsync(request, cancellationToken);
            request.ExclusiveStartKey = response.LastEvaluatedKey;

            list.AddRange(response.Items.Select(_createItem));

        } while (response.LastEvaluatedKey.Count > 0);

        return list;
    }
}
