using Amazon.DynamoDBv2;
using Amazon.Runtime;

namespace Dynatello.Pipelines;

internal static class RequestPipelineExtensions
{
    internal static Task<TResponse> SendRequest<TRequest, TResponse>(
        this TRequest request,
        IList<IRequestPipeLine> pipelines,
        Func<TRequest, IAmazonDynamoDB, CancellationToken, Task<TResponse>> invocation,
        IAmazonDynamoDB dynamoDb,
        CancellationToken cancellationToken
    )
        where TRequest : AmazonDynamoDBRequest
        where TResponse : AmazonWebServiceResponse
    {
        return pipelines.Count > 0
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
            var requestPipeLine = pipelines.Compose(async x => ReferenceEquals(x, requestContext) is false
                ? throw new InvalidOperationException(
                    $"Request context is not the same object, make sure to pass on the {nameof(RequestContext)}."
                )
                : await invocation((TRequest)x.Request, dynamoDb, cancellationToken));

            return (TResponse)await requestPipeLine(requestContext);
        }
    }

    private static Func<RequestContext, Task<AmazonWebServiceResponse>> Compose(
        this IEnumerable<IRequestPipeLine> pipelines,
        Func<RequestContext, Task<AmazonWebServiceResponse>> request
    ) => pipelines
            .Select<IRequestPipeLine,Func<RequestContext, Func<RequestContext, Task<AmazonWebServiceResponse>>, Task<AmazonWebServiceResponse>>>(x => x.Invoke)
            .Reverse()
            .Aggregate(
                request,
                (execution, continuation) =>
                    requestContext => continuation(requestContext, execution)
            );
}