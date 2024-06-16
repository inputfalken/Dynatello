
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;
using Dynatello.Builders;

namespace Dynatello.Handlers;

public static class Extensions
{
    /// Create a <see cref="PutRequestHandler{T}"/>
    public static IRequestHandler<IReadOnlyList<T>, TArg> ToQueryRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, QueryRequest>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new QueryRequestHandler<T, TArg>(dynamoDb, requestBuilderSelector(item.ToRequestBuilderFactory()).Build, item.Marshaller.Unmarshall);
    }

    /// Create a <see cref="PutRequestHandler{T}"/>
    public static IRequestHandler<T?, TArg> ToUpdateRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, UpdateItemRequest>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new UpdateRequestHandler<T, TArg>(dynamoDb, requestBuilderSelector(item.ToRequestBuilderFactory()).Build, item.Marshaller.Unmarshall);
    }

    /// Create a <see cref="PutRequestHandler{T}"/>
    public static IRequestHandler<T?, T> ToPutRequestHandler<T, TArg, TReferences, TArgumentReferences>(
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

    /// Create a <see cref="GetRequestHandler{T, TArg}"/>
    public static IRequestHandler<T?, TArg> ToGetRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, GetItemRequest>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new GetRequestHandler<T, TArg>(dynamoDb, requestBuilderSelector(item.ToRequestBuilderFactory()).Build, item.Marshaller.Unmarshall);
    }

}
