using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace Dynatello.Builders;

/// <summary>
/// A <see cref="QueryRequest"/> builder that can be configured through the record `with` syntax.
/// </summary>
/// <typeparam name="T">
/// The type you need to provide in you execution.
/// </typeparam>
public readonly record struct QueryRequestBuilder<T> : IRequestBuilder<T, QueryRequest>
{
    private readonly Func<T, IAttributeExpression> _attributeExpressionSelector;
    private readonly string _tableName;

    internal QueryRequestBuilder(
        Func<T, IAttributeExpression> attributeExpressionSelector,
        string tableName
    )
    {
        _attributeExpressionSelector = attributeExpressionSelector;
        _tableName = tableName;
    }

    /// <inheritdoc cref="QueryRequest.TableName" />
    public string TableName
    {
        get => _tableName;
        init => _tableName = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc cref="QueryRequest.IndexName" />
    public string? IndexName { get; init; } = null;

    /// <inheritdoc cref="QueryRequest.Limit" />
    public int? Limit { get; init; } = null;

    /// <inheritdoc cref="QueryRequest.ConsistentRead" />
    public bool? ConsistentRead { get; init; } = null;

    /// <inheritdoc cref="QueryRequest.ScanIndexForward" />
    public bool? ScanIndexForward { get; init; } = null;

    /// <inheritdoc cref="QueryRequest.Select" />
    public Select? Select { get; init; } = null;

    /// <inheritdoc cref="QueryRequest.ReturnConsumedCapacity" />
    public ReturnConsumedCapacity? ReturnConsumedCapacity { get; init; } = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public QueryRequest Build(T arg)
    {
        var attributeExpression = _attributeExpressionSelector(arg);

        var queryRequest = new QueryRequest
        {
            AttributesToGet = null,
            QueryFilter = null,
            ConditionalOperator = null,
            KeyConditions = null,
            KeyConditionExpression = attributeExpression.Expressions[0],
            ExpressionAttributeValues = attributeExpression.Values,
            ExpressionAttributeNames = attributeExpression.Names,
            TableName = TableName,
            IndexName = null,
            ProjectionExpression = null,
        };

        if (ReturnConsumedCapacity is not null)
            queryRequest.ReturnConsumedCapacity = ReturnConsumedCapacity;

        if (ConsistentRead is { } consistentRead)
            queryRequest.ConsistentRead = consistentRead;

        if (ScanIndexForward is { } scanIndexForward)
            queryRequest.ScanIndexForward = scanIndexForward;

        if (Select is not null)
            queryRequest.Select = Select;

        if (Limit is { } limit)
            queryRequest.Limit = limit;
        else if (IndexName is not null)
            queryRequest.IndexName = IndexName;

        if (attributeExpression.Expressions.Count == 2)
            queryRequest.FilterExpression = attributeExpression.Expressions[1];

        return queryRequest;
    }
}
