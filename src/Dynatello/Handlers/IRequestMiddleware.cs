
using Amazon.DynamoDBv2;

namespace Dynatello.Handlers;

public interface IRequestMiddleware<out T> where T : AmazonDynamoDBRequest
{
    /// <summary>
    /// Configure the <typeparamref name="T"/>.
    /// </summary>
    public void Configure(Action<T> configure);
}
