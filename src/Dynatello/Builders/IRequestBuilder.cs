using Amazon.DynamoDBv2;

namespace Dynatello.Builders;

public interface IRequestBuilder<T, TRequest> where TRequest : AmazonDynamoDBRequest
{
    public TRequest Build(T arg);
}
