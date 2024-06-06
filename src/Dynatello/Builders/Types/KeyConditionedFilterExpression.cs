using DynamoDBGenerator;
using Dynatello.Handlers;

namespace Dynatello.Builders.Types;

/// <summary>
/// Represents a AttributeExpression with both a key condition and a filter.
/// </summary>
public readonly struct KeyConditionedFilterExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly Func<TReferences, TArgumentReferences, string> Filter;
    internal readonly IRequestBuilder<T, TArg, TReferences, TArgumentReferences> Builder;

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public KeyConditionedFilterExpression()
    {
        throw Constants.InvalidConstructor();
    }

    internal KeyConditionedFilterExpression(
        in IRequestBuilder<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> condition,
        in Func<TReferences, TArgumentReferences, string> filter
    )
    {
        Builder = tableAccess;
        Condition = condition;
        Filter = filter;
    }
}
