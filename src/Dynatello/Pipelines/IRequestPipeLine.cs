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
    internal static Task<TResponse> SendRequest<TRequest, TResponse>(
        this TRequest request,
        IEnumerable<IRequestPipeLine> pipelines,
        Func<TRequest, IAmazonDynamoDB, CancellationToken, Task<TResponse>> invocation,
        IAmazonDynamoDB dynamoDb,
        CancellationToken cancellationToken
        ) where TRequest : AmazonDynamoDBRequest where TResponse : AmazonWebServiceResponse
    {
        return pipelines.ShouldIterate()
          ? WithPipeline(request, pipelines, dynamoDb, invocation, cancellationToken)
          : invocation(request, dynamoDb, cancellationToken);

        static async Task<TResponse> WithPipeline(
            TRequest request,
            IEnumerable<IRequestPipeLine> pipelines,
            IAmazonDynamoDB dynamoDb,
            Func<TRequest, IAmazonDynamoDB, CancellationToken, Task<TResponse>> invocation,
            CancellationToken cancellationToken
        )
        {

            var requestContext = new RequestContext<TRequest>(request, cancellationToken);
            var requestPipeLine = pipelines.Compose(async x =>
            {
                return object.ReferenceEquals(x, requestContext) is false
                                  ? throw new InvalidOperationException($"Request context is not the same object, make sure to pass on the {nameof(RequestContext)}.")
                                  : await invocation((TRequest)x.Request, dynamoDb, cancellationToken);
            });

            return (TResponse)(await requestPipeLine(requestContext));
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

    private static bool ShouldIterate<T>(this IEnumerable<T> pipelines)
    {
        return pipelines switch
        {
            _ when pipelines == Enumerable.Empty<T>() => false,
            _ when pipelines == Array.Empty<T>() => false,
            _ when pipelines.TryGetNonEnumeratedCount(out var count) => count != 0,
            _ => true
        };
    }
}
