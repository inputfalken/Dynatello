using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

/// <summary>
/// Represents a AttributeExpression with a condition.
/// </summary>
public readonly record struct ConditionExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> Builder;

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public ConditionExpression()
    {
        throw Constants.InvalidConstructor();
    }

    internal ConditionExpression(
        in IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> condition
    )
    {
        Builder = tableAccess;
        Condition = condition;
    }
}
