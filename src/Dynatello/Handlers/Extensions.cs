
using Amazon.DynamoDBv2;
using DynamoDBGenerator;
using Dynatello.Builders;

namespace Dynatello.Handlers;

public static class Extensions
{

    /// Create a <see cref="PutRequestHandler{T}"/>
    public static IRequestHandler<TArg> WithUpdateRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<TableAccess<T, TArg, TReferences, TArgumentReferences>, UpdateRequestBuilder<TArg>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {

        var requestBuilder = requestBuilderSelector(item);

        return new UpdateRequestHandler<TArg>(dynamoDb, requestBuilder.Build);
    }

    /// Create a <see cref="PutRequestHandler{T}"/>
    public static IRequestHandler<T, T> WithPutRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<TableAccess<T, TArg, TReferences, TArgumentReferences>, PutRequestBuilder<T>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {

        var requestBuilder = requestBuilderSelector(item);
        return new PutRequestHandler<T>(dynamoDb, requestBuilder.Build, item.Item.Unmarshall);
    }

    /// Create a <see cref="GetRequestHandler{T, TArg}"/>
    public static IRequestHandler<T, TArg> WithGetRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<TableAccess<T, TArg, TReferences, TArgumentReferences>, GetRequestBuilder<TArg>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {

        var requestBuilder = requestBuilderSelector(item);
        return new GetRequestHandler<T, TArg>(dynamoDb, requestBuilder.Build, item.Item.Unmarshall);
    }

}
