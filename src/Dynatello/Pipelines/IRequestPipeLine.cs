using Amazon.Runtime;

namespace Dynatello.Pipelines;

/// <summary>
/// 
/// </summary>
public interface IRequestPipeLine
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestContext"></param>
    /// <param name="continuation"></param>
    /// <returns></returns>
    public Task<AmazonWebServiceResponse> Invoke(
        RequestContext requestContext,
        Func<RequestContext, Task<AmazonWebServiceResponse>> continuation
    );
}
