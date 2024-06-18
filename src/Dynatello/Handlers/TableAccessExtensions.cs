
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;
using Dynatello.Builders;
using Dynatello.Pipelines;

namespace Dynatello.Handlers;

/// <summary>
/// Contains static extension methods for creating <see cref="IRequestHandler{T, TArg}"/>.
/// </summary>
public static class TableAccessExtensions
{
    /// <summary>
    /// Creates a <see cref="QueryRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<TArg, IReadOnlyList<T>> ToQueryRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, QueryRequest>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new QueryRequestHandler<TArg, T>(dynamoDb, requestBuilderSelector(item.ToRequestBuilderFactory()).Build, item.Marshaller.Unmarshall);
    }

    /// <summary>
    /// Creates a <see cref="UpdateItemRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<TArg, T?> ToUpdateRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, UpdateItemRequest>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new UpdateRequestHandler<TArg, T>(dynamoDb, requestBuilderSelector(item.ToRequestBuilderFactory()).Build, item.Marshaller.Unmarshall);
    }

    /// <summary>
    /// Creates a <see cref="PutItemRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<T, T?> ToPutRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<T, PutItemRequest>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new PutRequestHandler<T>(dynamoDb, requestBuilderSelector(item.ToRequestBuilderFactory()).Build, item.Marshaller.Unmarshall);
    }

    /// <summary>
    /// Creates a <see cref="GetItemRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<TArg, T?> ToGetRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, GetItemRequest>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new GetRequestHandler<TArg, T>(
            dynamoDb,
            requestBuilderSelector(item.ToRequestBuilderFactory()).Build,
            item.Marshaller.Unmarshall,
            Enumerable.Empty<IRequestPipeLine>()
        );
    }

}
