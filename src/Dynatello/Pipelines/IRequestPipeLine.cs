
using Amazon.DynamoDBv2;

namespace Dynatello.Pipelines;

public interface IRequestPipeLine
{
    public Task Invoke(RequestContext requestContext, Func<RequestContext, Task> continuation);
}

public readonly record struct RequestContext(AmazonDynamoDBRequest Request);

internal static class RequestPipelineExtensons
{
    internal static Task Bind(this IEnumerable<IRequestPipeLine> pipelines, RequestContext context)
    {
        return Task.CompletedTask;
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
