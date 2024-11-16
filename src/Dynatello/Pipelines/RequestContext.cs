using Amazon.DynamoDBv2;

namespace Dynatello.Pipelines;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public sealed record RequestContext<TRequest> : RequestContext
    where TRequest : AmazonDynamoDBRequest
{
    internal RequestContext(TRequest request, CancellationToken cancellationToken)
        : base(request, cancellationToken)
    {
        Request = request;
    }

    public TRequest Request { get; }
}

public record RequestContext
{
    internal RequestContext(AmazonDynamoDBRequest request, CancellationToken cancellationToken)
    {
        Request = request;
        CancellationToken = cancellationToken;
    }

    public AmazonDynamoDBRequest Request { get; }
    public CancellationToken CancellationToken { get; }
}
