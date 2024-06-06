using DynamoDBGenerator;
using Dynatello.Handlers;

namespace Dynatello.Builders.Types;

/// <summary>
/// Represents a AttributeExpression with a key condition.
/// </summary>
public readonly struct KeyConditionExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly Func<TReferences, TArgumentReferences, string> Condition;
    internal readonly IRequestBuilder<T, TArg, TReferences, TArgumentReferences> Builder;

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public KeyConditionExpression()
    {
        throw Constants.InvalidConstructor();
    }

    internal KeyConditionExpression(
        in IRequestBuilder<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> condition
    )
    {
        Builder = tableAccess;
        Condition = condition;
    }
}
