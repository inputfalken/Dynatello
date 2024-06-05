using Amazon.Runtime;

namespace Dynatello.Handlers;

/// <summary>
/// 
/// </summary>
public interface IResponseMiddleware<out T> where T : AmazonWebServiceResponse
{
    /// <summary>
    /// Configure the <typeparamref name="T"/> .
    /// </summary>
    public void Configure(Action<T> configure);
}
