using DynamoDBGenerator;
using Dynatello.Builders.Types;

namespace Dynatello;

/// <summary>
/// 
/// </summary>
public static class Extensions
{

    /// <summary>
    /// Creates a <see cref="TableAccess{T,TArg,TReferences,TArgumentReferences}"/> that's used for configuring builders.
    /// </summary>
    /// <param name="item">
    /// A <see cref="IDynamoDBMarshaller{TEntity,TArgument,TEntityAttributeNameTracker,TArgumentAttributeValueTracker}"/>.
    /// </param>
    /// <param name="tableName">
    /// The table that you want to perform operations onto.
    /// </param>
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
    public static TableAccess<T, TArg, TReferences, TArgumentReferences> OnTable
        <T, TArg, TReferences, TArgumentReferences>
        (this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item, string tableName)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new TableAccess<T, TArg, TReferences, TArgumentReferences>(in tableName, in item);
    }
}
