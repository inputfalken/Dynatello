using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Dynatello.Builders;

/// <summary>
/// A <see cref="GetItemRequest"/> builder that can be configured through the record `with` syntax.
/// </summary>
/// <typeparam name="T">
/// The type you need to provide in you execution.
/// </typeparam>
public readonly record struct GetRequestBuilder<T> : IRequestBuilder<T, GetItemRequest>
{
    private readonly Func<T, Dictionary<string, AttributeValue>> _keysSelector;
    private readonly string _tableName;

    internal GetRequestBuilder(
        string tableName,
        Func<T, Dictionary<string, AttributeValue>> keysSelector
    )
    {
        _keysSelector = keysSelector;
        _tableName = tableName;
    }

    /// <inheritdoc cref="GetItemRequest.TableName" />
    public string TableName
    {
        get => _tableName;
        init => _tableName = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc cref="GetItemRequest.ConsistentRead" />
    public bool? ConsistentRead { get; init; } = null;

    /// <inheritdoc cref="GetItemRequest.ReturnConsumedCapacity" />
    public ReturnConsumedCapacity? ReturnConsumedCapacity { get; init; } = null;

    /// <summary>
    /// Creates a <see cref="GetItemRequest"/>.
    /// </summary>
    /// <param name="arg">
    /// The argument required to extract the keys.
    /// </param>
    /// <returns>A <see cref="GetItemRequest"/></returns>
    public GetItemRequest Build(T arg)
    {
        var request = new GetItemRequest
        {
            ReturnConsumedCapacity = ReturnConsumedCapacity,
            TableName = TableName,
            Key = _keysSelector(arg),
        };

        if (ConsistentRead is { } consistentRead)
            request.ConsistentRead = consistentRead;

        if (ReturnConsumedCapacity is not null)
            request.ReturnConsumedCapacity = ReturnConsumedCapacity;

        return request;
    }
}
