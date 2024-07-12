using DynamoDBGenerator;
using Dynatello.Builders;

namespace Dynatello;

/// <summary>
/// Contains static extension methods from
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Creates a <see cref="TableAccess{T,TArg,TReferences,TArgumentReferences}"/>.
    /// </summary>
    public static ITableAccess<T, TArg, TReferences, TArgumentReferences> OnTable<
        T,
        TArg,
        TReferences,
        TArgumentReferences
    >(this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item, string tableName)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new TableAccess<T, TArg, TReferences, TArgumentReferences>(in tableName, in item);
    }

    /// <summary>
    /// Creates a <see cref="IRequestBuilderFactory{T, TArg, TReferences, TArgumentReferences}"/> that's used setting up request builders.
    /// </summary>
    public static IRequestBuilderFactory<
        T,
        TArg,
        TReferences,
        TArgumentReferences
    > ToRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item
    )
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        if (item is TableAccess<T, TArg, TReferences, TArgumentReferences> tableAccess)
            return tableAccess;

        throw new NotImplementedException("Custom implementation of ITableAccess.");
    }
}
