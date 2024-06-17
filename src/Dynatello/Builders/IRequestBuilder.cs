using Amazon.DynamoDBv2;

namespace Dynatello.Builders;

/// <summary>
/// Represents a request builder for AmazaonDynamoDB.
/// </summary>
public interface IRequestBuilder<T, TRequest> where TRequest : AmazonDynamoDBRequest
{
    /// <summary>
    /// Builds a from the argument provided.
    /// </summary>
    public TRequest Build(T arg);
}

