using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;
using Dynatello.Builders.Types;

namespace Dynatello.Builders;

/// <summary>
/// Contains extension methods to create builders.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static QueryRequestBuilder<TArg> ToQueryRequestBuilder<T, TArg, TReferences, TArgumentReferences>(
        this KeyConditionedFilterExpression<T, TArg, TReferences, TArgumentReferences> source
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new QueryRequestBuilder<TArg>(
            source.Builder.TableAccess.Marshaller.ComposeAttributeExpression(source.Condition, source.Filter),
            source.Builder.TableAccess.TableName
        );
    }

    /// <summary>
    /// 
    /// </summary>
    public static QueryRequestBuilder<TArg> ToQueryRequestBuilder<T, TArg, TReferences, TArgumentReferences>(
        this KeyConditionExpression<T, TArg, TReferences, TArgumentReferences> source
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new QueryRequestBuilder<TArg>(
            source.Builder.TableAccess.Marshaller.ComposeAttributeExpression(source.Condition, null),
            source.Builder.TableAccess.TableName
        );
    }

    /// <summary>
    /// 
    /// </summary>
    public static GetRequestBuilder<TArg> ToGetRequestBuilder<T, TArg, TReferences, TArgumentReferences>(
        this IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> source)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
        where TArg : notnull
    {
        return new GetRequestBuilder<TArg>(
            source.TableAccess.TableName,
            source.TableAccess.Marshaller.PrimaryKeyMarshaller.ComposeKeys<TArg>(y => y, null)
        );
    }

    /// <summary>
    /// 
    /// </summary>
    public static GetRequestBuilder<TArg> ToGetRequestBuilder<T, TArg, TReferences, TArgumentReferences,
        TPartition>(
        this IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> source,
        Func<TArg, TPartition> partitionKeySelector)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
        where TPartition : notnull
    {
        return new GetRequestBuilder<TArg>(
            source.TableAccess.TableName,
            source.TableAccess.Marshaller.PrimaryKeyMarshaller.ComposeKeys<TArg>(y => partitionKeySelector(y), null)
        );
    }

    /// <summary>
    /// 
    /// </summary>
    public static GetRequestBuilder<TArg> ToGetRequestBuilder<T, TArg,
        TReferences, TArgumentReferences, TPartition, TRange>(
        this IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> source,
        Func<TArg, TPartition> partitionKeySelector,
        Func<TArg, TRange> rangeKeySelector)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
        where TPartition : notnull
        where TRange : notnull
    {
        return new GetRequestBuilder<TArg>(
            source.TableAccess.TableName,
            source.TableAccess.Marshaller.PrimaryKeyMarshaller.ComposeKeys<TArg>(y => partitionKeySelector(y), y => rangeKeySelector(y))
        );
    }

    /// <summary>
    /// 
    /// </summary>
    public static UpdateRequestBuilder<TArg> ToUpdateItemRequestBuilder<T, TArg, TReferences, TArgumentReferences>(
        this UpdateExpression<T, TArg, TReferences, TArgumentReferences> source,
        Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new UpdateRequestBuilder<TArg>(
            source.Builder.TableAccess.Marshaller.ComposeAttributeExpression(source.Update, null),
            source.Builder.TableAccess.TableName,
            keySelector,
            source.Builder.TableAccess.Marshaller.PrimaryKeyMarshaller
        );
    }

    /// <summary>
    /// 
    /// </summary>
    public static UpdateRequestBuilder<TArg> ToUpdateItemRequestBuilder<T, TArg, TReferences, TArgumentReferences>(
        this ConditionalUpdateExpression<T, TArg, TReferences, TArgumentReferences> source,
        Func<IDynamoDBKeyMarshaller, TArg, Dictionary<string, AttributeValue>> keySelector
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new UpdateRequestBuilder<TArg>(
            source.Builder.TableAccess.Marshaller.ComposeAttributeExpression(source.Update, source.Condition),
            source.Builder.TableAccess.TableName,
            keySelector,
            source.Builder.TableAccess.Marshaller.PrimaryKeyMarshaller
        );
    }

    /// <summary>
    /// 
    /// </summary>
    public static KeyConditionExpression<T, TArg, TReferences, TArgumentReferences> WithKeyConditionExpression<T, TArg,
        TReferences, TArgumentReferences>(
        this IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> condition)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new KeyConditionExpression<T, TArg, TReferences, TArgumentReferences>(source, condition);
    }

    /// <summary>
    /// 
    /// </summary>
    public static PutRequestBuilder<T> ToPutRequestBuilder<T, TReferences,
        TArgumentReferences>(
        this IRequestBuilderFactory<T, T, TReferences, TArgumentReferences> source
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<T>
    {
        return new PutRequestBuilder<T>
        (
            null,
            source.TableAccess.Marshaller.Marshall,
            source.TableAccess.TableName
        );
    }

    /// <summary>
    /// 
    /// </summary>
    public static PutRequestBuilder<T> ToPutRequestBuilder<T, TReferences,
        TArgumentReferences>(
        this ConditionExpression<T, T, TReferences, TArgumentReferences> source
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<T>
    {
        return new PutRequestBuilder<T>
        (
            source.Builder.TableAccess.Marshaller.ComposeAttributeExpression(null, source.Condition),
            source.Builder.TableAccess.Marshaller.Marshall,
            source.Builder.TableAccess.TableName
        );
    }
}
