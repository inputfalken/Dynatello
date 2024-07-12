using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace Dynatello.Builders;

/// <summary>
/// A <see cref="DeleteItemRequest"/> builder that can be configured through the record `with` syntax.
/// </summary>
/// <typeparam name="T">
/// The type you need to provide in you execution.
/// </typeparam>
public readonly record struct DeleteRequestBuilder<T> : IRequestBuilder<T, DeleteItemRequest>
{
    private readonly Func<T, IAttributeExpression>? _attributeExpressionSelector;
    private readonly Func<T, Dictionary<string, AttributeValue>> _keySelector;
    private readonly string _tableName;

    internal DeleteRequestBuilder(
        string tableName,
        Func<T, Dictionary<string, AttributeValue>> keySelector,
        Func<T, IAttributeExpression>? attributeExpressionSelector
    )
    {
        _attributeExpressionSelector = attributeExpressionSelector;
        _tableName = tableName;
        _keySelector = keySelector;
    }

    /// <inheritdoc cref="DeleteItemRequest.TableName" />
    public string TableName
    {
        get => _tableName;
        init => _tableName = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc cref="DeleteItemRequest.ReturnConsumedCapacity" />
    public ReturnConsumedCapacity? ReturnConsumedCapacity { get; init; } = null;

    /// <inheritdoc cref="DeleteItemRequest.ReturnItemCollectionMetrics" />
    public ReturnItemCollectionMetrics? ReturnItemCollectionMetrics { get; init; } = null;

    /// <inheritdoc cref="DeleteItemRequest.ReturnValues" />
    public ReturnValue? ReturnValues { get; init; } = null;

    /// <inheritdoc cref="DeleteItemRequest.ReturnValuesOnConditionCheckFailure" />
    public ReturnValuesOnConditionCheckFailure? ReturnValuesOnConditionCheckFailure { get; init; } =
        null;

    /// <inheritdoc cref="IRequestBuilder{T, TRequest}.Build(T)"/>
    public DeleteItemRequest Build(T arg)
    {
        var request = new DeleteItemRequest
        {
            TableName = TableName,
            Key = _keySelector(arg),
            ConditionalOperator = null,
        };

        if (_attributeExpressionSelector?.Invoke(arg) is { } expression)
        {
            request.ExpressionAttributeNames = expression.Names;
            request.ExpressionAttributeValues = expression.Values;
            request.ConditionExpression = expression.Expressions[0];
        }

        if (ReturnValues is not null)
            request.ReturnValues = ReturnValues;

        if (ReturnConsumedCapacity is not null)
            request.ReturnConsumedCapacity = ReturnConsumedCapacity;

        if (ReturnItemCollectionMetrics is not null)
            request.ReturnItemCollectionMetrics = ReturnItemCollectionMetrics;

        if (ReturnValuesOnConditionCheckFailure is not null)
            request.ReturnValuesOnConditionCheckFailure = ReturnValuesOnConditionCheckFailure;

        return request;
    }
}
