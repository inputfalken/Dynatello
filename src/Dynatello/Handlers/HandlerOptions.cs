
using Amazon.DynamoDBv2;
using Dynatello.Pipelines;

namespace Dynatello.Handlers;

public sealed class HandlerOptions
{
    private AmazonDynamoDBClient defaultClient;
    private IAmazonDynamoDB? client;
    private IAmazonDynamoDB DefaultClient => defaultClient ??= new AmazonDynamoDBClient();

    public IAmazonDynamoDB AmazonDynamoDB
    {
        get
        {

            return client ?? DefaultClient;
        }
        set { client = value; }
    }

    public IList<IRequestPipeLine> RequestsPipelines { get; } = new List<IRequestPipeLine>();
}
