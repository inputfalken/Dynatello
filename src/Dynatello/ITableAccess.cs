using DynamoDBGenerator;

namespace Dynatello;

/// <summary>
/// Represents the combination of a Marshaller together with a TableName.
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
public interface ITableAccess<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    public IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> Marshaller { get; }
    public string TableName { get; }
}


