using Amazon.Runtime;

namespace Dynatello.Handlers;

public interface IResponseHandler<out T> where T : AmazonWebServiceResponse
{
    /// <summary>
    /// Configure the <typeparamref name="T"/> .
    /// </summary>
    public void Configure(Action<T> configure);
}
