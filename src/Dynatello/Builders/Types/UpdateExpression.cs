using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

/// <summary>
/// Represents a AttributeExpression with a update.
/// </summary>
public class UpdateExpression<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal readonly IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> Builder;
    internal readonly Func<TReferences, TArgumentReferences, string> Update;

    internal UpdateExpression(
        in IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences> tableAccess,
        in Func<TReferences, TArgumentReferences, string> update
    )
    {
        Builder = tableAccess;
        Update = update;
    }
}
