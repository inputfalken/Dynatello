using Amazon.DynamoDBv2;

namespace Dynatello.Builders;

/// <summary>
/// Represents a request builder for AmazaonDynamoDB.
/// </summary>
public interface IRequestBuilder<in T, out TRequest>
    where TRequest : AmazonDynamoDBRequest
{
    /// <summary>
    /// Builds a <typeparamref name="TRequest"/> from <typeparamref name="T"/>.
    /// </summary>
    public TRequest Build(T arg);
}
