using Amazon.Runtime;

namespace Dynatello.Pipelines;

/// <summary>
/// 
/// </summary>
public interface IRequestPipeLine
{
    public Task<AmazonWebServiceResponse> Invoke(
        RequestContext requestContext,
        Func<RequestContext, Task<AmazonWebServiceResponse>> continuation
    );
}
