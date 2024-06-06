using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

/// <summary>
/// Represents a AttributeExpression with both an update and condition.
/// </summary>
public readonly record struct ConditionalUpdateExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> Builder;
    internal readonly Func<TReferences, TArgumentReferences, string> Update;

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public ConditionalUpdateExpression()
    {
        throw Constants.InvalidConstructor();
    }

    internal ConditionalUpdateExpression(
        in IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> update,
        in Func<TReferences, TArgumentReferences, string> condition)
    {
        Builder = tableAccess;
        Update = update;
        Condition = condition;
    }
}
