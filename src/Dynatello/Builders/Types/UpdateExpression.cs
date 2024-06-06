using DynamoDBGenerator;
using Dynatello.Handlers;

namespace Dynatello.Builders.Types;

/// <summary>
/// Represents a AttributeExpression with a update.
/// </summary>
public readonly record struct UpdateExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly IRequestBuilder<T, TArg, TReferences, TArgumentReferences> Builder;
    internal readonly Func<TReferences, TArgumentReferences, string> Update;

    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public UpdateExpression()
    {
        throw Constants.InvalidConstructor();
    }

    internal UpdateExpression(
        in IRequestBuilder<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> update)
    {
        Builder = tableAccess;
        Update = update;
    }
}
