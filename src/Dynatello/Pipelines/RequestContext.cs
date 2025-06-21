using Amazon.DynamoDBv2;

namespace Dynatello.Pipelines;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public sealed record RequestContext<TRequest> : RequestContext
    where TRequest : AmazonDynamoDBRequest
{
    internal RequestContext(TRequest request, CancellationToken cancellationToken) : base(request, cancellationToken)
    {
    }
}

/// <summary>
/// 
/// </summary>
public record RequestContext
{
    internal RequestContext(AmazonDynamoDBRequest request, CancellationToken cancellationToken)
    {
        Request = request;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// 
    /// </summary>
    public AmazonDynamoDBRequest Request { get; }
    /// <summary>
    /// 
    /// </summary>
    public CancellationToken CancellationToken { get; }
}