
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;
using Dynatello.Builders;

namespace Dynatello.Handlers;

public static class Extensions
{
    /// Create a <see cref="PutRequestHandler{T}"/>
    public static IRequestHandler<IReadOnlyList<T>, TArg> WithQueryRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<TableAccess<T, TArg, TReferences, TArgumentReferences>, QueryRequestBuilder<TArg>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new QueryRequestHandler<T, TArg>(dynamoDb, requestBuilderSelector(item).Build, item.Item.Unmarshall);
    }
    
    /// Create a <see cref="PutRequestHandler{T}"/>
    public static IRequestHandler<UpdateItemResponse, TArg> WithUpdateRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<TableAccess<T, TArg, TReferences, TArgumentReferences>, UpdateRequestBuilder<TArg>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new UpdateRequestHandler<TArg>(dynamoDb, requestBuilderSelector(item).Build);
    }

    /// Create a <see cref="PutRequestHandler{T}"/>
    public static IRequestHandler<T?, T> WithPutRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<TableAccess<T, TArg, TReferences, TArgumentReferences>, PutRequestBuilder<T>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new PutRequestHandler<T>(dynamoDb, requestBuilderSelector(item).Build, item.Item.Unmarshall);
    }

    /// Create a <see cref="GetRequestHandler{T, TArg}"/>
    public static IRequestHandler<T?, TArg> WithGetRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<TableAccess<T, TArg, TReferences, TArgumentReferences>, GetRequestBuilder<TArg>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new GetRequestHandler<T, TArg>(dynamoDb, requestBuilderSelector(item).Build, item.Item.Unmarshall);
    }

}
