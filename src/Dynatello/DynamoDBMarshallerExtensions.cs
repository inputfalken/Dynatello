using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using Dynatello.Handlers;
using static DynamoDBGenerator.Extensions.DynamoDBMarshallerExtensions;

namespace Dynatello;

/// <summary>
/// 
/// </summary>
public static class DynamoDBMarshallerExtensions
{
    internal static Func<TArg, Dictionary<string, AttributeValue>> ComposeKeys<TArg>(
        this IDynamoDBKeyMarshaller source,
        Func<TArg, object> partitionKeySelector,
        Func<TArg, object>? rangeKeySelector
    )
    {
        return (partitionKeySelector, rangeKeySelector) switch
        {
            (not null, not null) => y => source.Keys(partitionKeySelector(y), rangeKeySelector(y)),
            (not null, null) => y => source.PartitionKey(partitionKeySelector(y)),
            (null, not null) => y => source.RangeKey(rangeKeySelector(y)),
            (null, null) => throw new ArgumentNullException(""),
        };
    }

    internal static Func<TArg, IAttributeExpression> ComposeAttributeExpression<
        T,
        TArg,
        TReferences,
        TArgumentReferences
    >(
        this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string>? update,
        Func<TReferences, TArgumentReferences, string>? condition
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return (update, condition) switch
        {
            (null, null) => throw new ArgumentNullException(""),
            (not null, not null) => y => source.ToAttributeExpression(y, update, condition),
            (not null, null) => y => source.ToAttributeExpression(y, update),
            (null, not null) => y => source.ToAttributeExpression(y, condition),
        };
    }
}
