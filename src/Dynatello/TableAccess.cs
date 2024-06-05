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
/// <returns>
/// A <see cref="TableAccess{T,TArg,TReferences,TArgumentReferences}"/> that can be used to chain behaviour through a builder pattern.
/// </returns>
public readonly record struct TableAccess<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public TableAccess()
    {
        throw Constants.InvalidConstructor();
    }

    internal TableAccess(in string tableName, in IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item)
    {
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        Marshaller = item ?? throw new ArgumentNullException(nameof(item));
    }

    internal readonly string TableName;
    internal readonly IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> Marshaller;
}
