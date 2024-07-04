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
    internal async static Task<TResponse> InvokeRequest<TRequest, TResponse>(
        this TRequest request,
        IEnumerable<IRequestPipeLine> pipelines,
        Func<TRequest, CancellationToken, Task<AmazonWebServiceResponse>> invocation,
        CancellationToken cancellationToken
        ) where TRequest : AmazonDynamoDBRequest where TResponse : AmazonWebServiceResponse
    {

        if (pipelines.IsEmpty() is false)
        {
            var requestContext = new RequestContext<TRequest>(request, cancellationToken);
            var requestPipeLine = pipelines.Compose(x =>
                object.ReferenceEquals(x, requestContext) is false
                  ? throw new InvalidOperationException($"Request context is not the same object, make sure to pass on the {nameof(RequestContext)}.")
                  : invocation((TRequest)x.Request, cancellationToken)
            );

            return (TResponse)(await requestPipeLine(requestContext));

        }
        else
        {
            return (TResponse)(await invocation(request, cancellationToken));
        }

    }

    private static Func<RequestContext, Task<AmazonWebServiceResponse>> Compose(
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
