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
    private readonly Func<T, IAttributeExpression> _attributeExpressionSelector;
    private readonly IDynamoDBKeyMarshaller _keyMarshaller;
    private readonly Func<IDynamoDBKeyMarshaller, T, Dictionary<string, AttributeValue>> _keySelector;
    private readonly string _tableName;

    internal DeleteRequestBuilder(
        Func<T, IAttributeExpression> attributeExpressionSelector,
        string tableName,
        Func<IDynamoDBKeyMarshaller, T, Dictionary<string, AttributeValue>> keySelector,
        IDynamoDBKeyMarshaller keyMarshaller)
    {
        _attributeExpressionSelector = attributeExpressionSelector;
        _tableName = tableName;
        _keySelector = keySelector;
        _keyMarshaller = keyMarshaller;
    }

    /// <inheritdoc cref="DeleteItemRequest.TableName" />
    public string TableName
    {
        get => _tableName;
        init => _tableName = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     A function to specify how the keys should be accessed through the <typeparamref name="T" />.
    /// </summary>
    public Func<IDynamoDBKeyMarshaller, T, Dictionary<string, AttributeValue>> KeySelector
    {
        get => _keySelector;
        init => _keySelector = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc cref="DeleteItemRequest.ReturnConsumedCapacity" />
    public ReturnConsumedCapacity? ReturnConsumedCapacity { get; init; } = null;

    /// <inheritdoc cref="DeleteItemRequest.ReturnItemCollectionMetrics" />
    public ReturnItemCollectionMetrics? ReturnItemCollectionMetrics { get; init; } = null;

    /// <inheritdoc cref="DeleteItemRequest.ReturnValues" />
    public ReturnValue? ReturnValues { get; init; } = null;

    /// <inheritdoc cref="DeleteItemRequest.ReturnValuesOnConditionCheckFailure" />
    public ReturnValuesOnConditionCheckFailure? ReturnValuesOnConditionCheckFailure { get; init; } = null;

    public DeleteItemRequest Build(T arg)
    {
        var expression = _attributeExpressionSelector(arg);
        var request = new DeleteItemRequest
        {
            TableName = TableName,
            Key = _keySelector(_keyMarshaller, arg),
            ExpressionAttributeNames = expression.Names,
            ExpressionAttributeValues = expression.Values,
            ConditionExpression = expression.Expressions[0],
            ConditionalOperator = null,
            Expected = null
        };

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


