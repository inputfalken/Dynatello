using Amazon.DynamoDBv2;
using Amazon.Runtime;

namespace Dynatello.Pipelines;

public interface IRequestPipeLine
{
    public Task<AmazonWebServiceResponse> Invoke(RequestContext requestContext, Func<RequestContext, Task<AmazonWebServiceResponse>> continuation);
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

public sealed record RequestContext<TRequest> : RequestContext where TRequest : AmazonDynamoDBRequest
{
    internal RequestContext(TRequest request, CancellationToken cancellationToken) : base(request, cancellationToken)
    {
        Request = request;
    }
    public TRequest Request { get; }
}

internal static class RequestPipelineExtensons
{
    internal static Func<RequestContext, Task<AmazonWebServiceResponse>> Compose(
        this IEnumerable<IRequestPipeLine> pipelines,
        Func<RequestContext, Task<AmazonWebServiceResponse>> request
      )
    {
        return pipelines
          .Select<IRequestPipeLine, Func<RequestContext, Func<RequestContext, Task<AmazonWebServiceResponse>>, Task<AmazonWebServiceResponse>>>(x => x.Invoke)
          .Reverse()
          .Aggregate(request, (execution, continuation) => requestContext => continuation(requestContext, execution));
    }

    internal static bool IsEmpty<T>(this IEnumerable<T> pipelines)
    {
        return pipelines switch
        {
            _ when pipelines == Enumerable.Empty<T>() => true,
            _ when pipelines == Array.Empty<T>() => true,
            _ when pipelines.TryGetNonEnumeratedCount(out var count) && count == 0 => true,
            _ => false
        };
    }
}
