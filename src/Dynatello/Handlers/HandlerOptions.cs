
using Amazon.DynamoDBv2;
using Dynatello.Pipelines;

namespace Dynatello.Handlers;

public sealed class HandlerOptions
{
    private AmazonDynamoDBClient? defaultClient;
    private IAmazonDynamoDB? client;


    public IAmazonDynamoDB AmazonDynamoDB
    {
        get
        {
            return client ??= defaultClient ??= new AmazonDynamoDBClient();
        }
        set { client = value; }
    }

    public IList<IRequestPipeLine> RequestsPipelines { get; } = new List<IRequestPipeLine>();
}
