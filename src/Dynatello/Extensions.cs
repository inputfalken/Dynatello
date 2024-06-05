using DynamoDBGenerator;
using Dynatello.Builders;

namespace Dynatello;

/// <summary>
/// 
/// </summary>
public static class Extensions
{

    /// <summary>
    /// Creates a <see cref="TableAccess{T,TArg,TReferences,TArgumentReferences}"/>.
    /// </summary>
    public static TableAccess<T, TArg, TReferences, TArgumentReferences> OnTable
        <T, TArg, TReferences, TArgumentReferences>
        (this IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item, string tableName)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new TableAccess<T, TArg, TReferences, TArgumentReferences>(in tableName, in item);
    }

    /// <summary>
    /// Creates a <see cref="RequestBuilder{T, TArg, TReferences, TArgumentReferences}"/> that's used setting up request builders.
    /// </summary>
    public static RequestBuilder<T, TArg, TReferences, TArgumentReferences> WithRequestBuilder
        <T, TArg, TReferences, TArgumentReferences>
        (this TableAccess<T, TArg, TReferences, TArgumentReferences> item)
        where TReferences : IAttributeExpressionNameTracker
        where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
    {
        return new RequestBuilder<T, TArg, TReferences, TArgumentReferences>(item);
    }
}
