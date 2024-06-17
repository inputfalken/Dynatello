using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

/// <summary>
/// Represents a AttributeExpression with a key condition.
/// </summary>
public class KeyConditionExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> Builder;

    internal KeyConditionExpression(
        in IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> condition
    )
    {
        Builder = tableAccess;
        Condition = condition;
    }
}
