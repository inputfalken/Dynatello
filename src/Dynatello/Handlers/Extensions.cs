
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;
using Dynatello.Builders;

namespace Dynatello.Handlers;

public static class Extensions
{
    /// Create a <see cref="PutRequestHandler{T}"/>
    public static IRequestHandler<IReadOnlyList<T>, TArg> WithQueryRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilder<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, QueryRequest>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new QueryRequestHandler<T, TArg>(dynamoDb, requestBuilderSelector(item.WithRequestBuilder()).Build, item.Marshaller.Unmarshall);
    }

    /// Create a <see cref="PutRequestHandler{T}"/>
    public static IRequestHandler<UpdateItemResponse, TArg> WithUpdateRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilder<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, UpdateItemRequest>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new UpdateRequestHandler<TArg>(dynamoDb, requestBuilderSelector(item.WithRequestBuilder()).Build);
    }

    /// Create a <see cref="PutRequestHandler{T}"/>
    public static IRequestHandler<T?, T> WithPutRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilder<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<T, PutItemRequest>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new PutRequestHandler<T>(dynamoDb, requestBuilderSelector(item.WithRequestBuilder()).Build, item.Marshaller.Unmarshall);
    }

    /// Create a <see cref="GetRequestHandler{T, TArg}"/>
    public static IRequestHandler<T?, TArg> WithGetRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilder<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, GetItemRequest>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new GetRequestHandler<T, TArg>(dynamoDb, requestBuilderSelector(item.WithRequestBuilder()).Build, item.Marshaller.Unmarshall);
    }

}
