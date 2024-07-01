using Amazon.DynamoDBv2;
using Amazon.Runtime;

namespace Dynatello.Pipelines;

public interface IRequestPipeLine
{
    public Task<AmazonWebServiceResponse> Invoke(RequestContext requestContext, Func<RequestContext, Task<AmazonWebServiceResponse>> continuation);
}

public readonly record struct RequestContext(AmazonDynamoDBRequest Request, CancellationToken CancellationToken);

internal static class RequestPipelineExtensons
{
    internal static Func<RequestContext, Task<AmazonWebServiceResponse>> Merge(
        this IEnumerable<IRequestPipeLine> funcs,
        Func<RequestContext, Task<AmazonWebServiceResponse>> request
      )
    {
        return funcs
          .Select<IRequestPipeLine, Func<RequestContext, Func<RequestContext, Task<AmazonWebServiceResponse>>, Task<AmazonWebServiceResponse>>>(x => x.Invoke)
          .Reverse()
          .Aggregate(request, (next, pipeline) => x => pipeline(x, y => next(y)));
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
