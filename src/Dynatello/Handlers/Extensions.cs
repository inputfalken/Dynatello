
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using DynamoDBGenerator;
using Dynatello.Builders;
using Dynatello.Builders.Types;

namespace Dynatello.Handlers;

public static class Extensions
{

    private static T OnRequest<T, TRequest>(T source, Action<TRequest> configure)
      where T : ITaskHandler, IRequestHandler<TRequest>
      where TRequest : AmazonDynamoDBRequest
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(configure);

        source.Configure(configure);
        return source;
    }
    private static T OnResponse<T, TResponse>(T source, Action<TResponse> configure)
      where T : ITaskHandler, IResponseHandler<TResponse>
      where TResponse : AmazonWebServiceResponse
    {
        source.Configure(configure);
        return source;
    }

    /// Create a <see cref="GetRequestHandler{T, TArg}"/>
    public static GetRequestHandler<T, TArg> WithGetRequestFactory<T, TArg, TReferences, TArgumentReferences>(
        this TableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<TableAccess<T, TArg, TReferences, TArgumentReferences>, GetRequestBuilder<TArg>> requestBuilderSelector,
        IAmazonDynamoDB dynamoDb
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
    {

        var requestBuilder = requestBuilderSelector(item);
        return new GetRequestHandler<T, TArg>(dynamoDb, requestBuilder.Build, item.Item.Unmarshall);
    }

    public static T OnRequest<T>(this T source, Action<GetItemRequest> configure)
      where T : ITaskHandler, IRequestHandler<GetItemRequest>
    {
        return OnRequest<T, GetItemRequest>(source, configure);
    }

    public static T OnResponse<T>(this T source, Action<GetItemResponse> configure)
      where T : ITaskHandler, IResponseHandler<GetItemResponse>
    {
        return OnResponse<T, GetItemResponse>(source, configure);
    }
}
