using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

/// <summary>
///
/// </summary>
public static class BuildingBlockExtensions
{
    /// <summary>
    ///
    /// </summary>
    public static KeyConditionedFilterExpression<
        T,
        TArg,
        TReferences,
        TArgumentReferences
    > WithFilterExpression<T, TArg, TReferences, TArgumentReferences>(
        this KeyConditionExpression<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> filter
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new KeyConditionedFilterExpression<T, TArg, TReferences, TArgumentReferences>(
            source.Builder,
            source.Condition,
            filter
        );
    }

    /// <summary>
    ///
    /// </summary>
    public static KeyConditionExpression<
        T,
        TArg,
        TReferences,
        TArgumentReferences
    > WithKeyConditionExpression<T, TArg, TReferences, TArgumentReferences>(
        this IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> condition
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new KeyConditionExpression<T, TArg, TReferences, TArgumentReferences>(
            source,
            condition
        );
    }

    /// <summary>
    ///
    /// </summary>
    public static ConditionalUpdateExpression<
        T,
        TArg,
        TReferences,
        TArgumentReferences
    > WithConditionExpression<T, TArg, TReferences, TArgumentReferences>(
        this UpdateExpression<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> condition
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new ConditionalUpdateExpression<T, TArg, TReferences, TArgumentReferences>(
            in source.Builder,
            in source.Update,
            condition
        );
    }

    /// <summary>
    ///
    /// </summary>
    public static UpdateExpression<T, TArg, TReferences, TArgumentReferences> WithUpdateExpression<
        T,
        TArg,
        TReferences,
        TArgumentReferences
    >(
        this IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> updateExpression
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new UpdateExpression<T, TArg, TReferences, TArgumentReferences>(
            in source,
            in updateExpression
        );
    }

    /// <summary>
    ///
    /// </summary>
    public static ConditionExpression<
        T,
        TArg,
        TReferences,
        TArgumentReferences
    > WithConditionExpression<T, TArg, TReferences, TArgumentReferences>(
        this IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> condition
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new ConditionExpression<T, TArg, TReferences, TArgumentReferences>(
            in source,
            in condition
        );
    }

    /// <summary>
    ///
    /// </summary>
    public static ConditionalUpdateExpression<
        T,
        TArg,
        TReferences,
        TArgumentReferences
    > WithUpdateExpression<T, TArg, TReferences, TArgumentReferences>(
        this ConditionExpression<T, TArg, TReferences, TArgumentReferences> source,
        Func<TReferences, TArgumentReferences, string> update
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new ConditionalUpdateExpression<T, TArg, TReferences, TArgumentReferences>(
            in source.Builder,
            in update,
            in source.Condition
        );
    }
}
