using DynamoDBGenerator;

namespace Dynatello.Builders;

/// <summary>
/// Represents a type to create various request builders from.
/// </summary>
/// <typeparam name="T">
/// The type that exists in the table.
/// </typeparam>
/// <typeparam name="TArg">
/// The argument that you want to provide through your execution.
/// </typeparam>
/// <typeparam name="TReferences">
/// A type that represents the type param <typeparamref name="T"/> with AttributeExpression support.
/// </typeparam>
/// <typeparam name="TArgumentReferences">
/// A type that represents the type param <typeparamref name="TArg"/> with AttributeExpression support.
/// </typeparam>
public interface IRequestBuilderFactory<T, in TArg, out TReferences, out TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    /// <summary>
    /// 
    /// </summary>
    public ITableAccess<T, TArg, TReferences, TArgumentReferences> TableAccess { get; }
}
