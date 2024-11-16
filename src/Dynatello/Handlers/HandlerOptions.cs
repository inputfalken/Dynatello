using Amazon.DynamoDBv2;
using Dynatello.Pipelines;

namespace Dynatello.Handlers;

/// <summary>
/// 
/// </summary>
public sealed class HandlerOptions
{
    private static IAmazonDynamoDB? _defaultClient;
    private IAmazonDynamoDB? _client;

    /// <summary>
    /// 
    /// </summary>
    public IAmazonDynamoDB AmazonDynamoDB
    {
        get => _client ??= _defaultClient ??= new AmazonDynamoDBClient();
        set => _client = value;
    }

    /// <summary>
    /// A <see cref="IList{T}"/> of request middlewares.
    /// </summary>
    public IList<IRequestPipeLine> RequestsPipelines { get; } = new List<IRequestPipeLine>();
}
