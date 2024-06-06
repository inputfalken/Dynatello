using DynamoDBGenerator;
using Dynatello.Builders;
using Dynatello.Handlers;

namespace Dynatello;

/// <summary>
/// 
/// </summary>
public static class Extensions
{

    /// <summary>
    /// Creates a <see cref="TableAccess{T,TArg,TReferences,TArgumentReferences}"/>.
    /// </summary>
    public static ITableAccess<T, TArg, TReferences, TArgumentReferences> OnTable
        <T, TArg, TReferences, TArgumentReferences>
        (this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item, string tableName)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new TableAccess<T, TArg, TReferences, TArgumentReferences>(in tableName, in item);
    }

    /// <summary>
    /// Creates a <see cref="IRequestBuilder{T, TArg, TReferences, TArgumentReferences}"/> that's used setting up request builders.
    /// </summary>
    public static IRequestBuilder<T, TArg, TReferences, TArgumentReferences> WithRequestBuilder
        <T, TArg, TReferences, TArgumentReferences>
        (this ITableAccess<T, TArg, TReferences, TArgumentReferences> item)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        if (item is TableAccess<T, TArg, TReferences, TArgumentReferences> tableAccess)
            return tableAccess;
        
        throw new NotImplementedException("Custom implementation of ITableAccess.");
    }
}
