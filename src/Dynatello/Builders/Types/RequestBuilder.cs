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
/// <returns>
/// A <see cref="TableAccess{T,TArg,TReferences,TArgumentReferences}"/> that can be used to chain behaviour through a builder pattern.
/// </returns>
public readonly record struct RequestBuilder<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public RequestBuilder()
    {
        throw Constants.InvalidConstructor();
    }

    internal RequestBuilder(TableAccess<T, TArg, TReferences, TArgumentReferences> tableAccess)
    {
        TableName = tableAccess.TableName;
        Marshaller = tableAccess.Marshaller;
    }

    internal readonly string TableName;
    internal readonly IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> Marshaller;
}
