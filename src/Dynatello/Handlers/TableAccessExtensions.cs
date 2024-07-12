
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;
using Dynatello.Builders;

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
        Action<HandlerOptions> configure
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        var options = new HandlerOptions();
        configure(options);
        return new QueryRequestHandler<TArg, T>(options, requestBuilderSelector(item.ToRequestBuilderFactory()).Build, item.Marshaller.Unmarshall);
    }
    /// <summary>
    /// Creates a <see cref="QueryRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<TArg, IReadOnlyList<T>> ToQueryRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, QueryRequest>> requestBuilderSelector
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new QueryRequestHandler<TArg, T>(new HandlerOptions(), requestBuilderSelector(item.ToRequestBuilderFactory()).Build, item.Marshaller.Unmarshall);
    }

    /// <summary>
    /// Creates a <see cref="UpdateItemRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<TArg, T?> ToUpdateRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, UpdateItemRequest>> requestBuilderSelector,
        Action<HandlerOptions> configure
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        var options = new HandlerOptions();
        configure(options);

        return new UpdateRequestHandler<TArg, T>(
            options,
            requestBuilderSelector(item.ToRequestBuilderFactory()).Build,
            item.Marshaller.Unmarshall
        );
    }
    /// <summary>
    /// Creates a <see cref="UpdateItemRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<TArg, T?> ToUpdateRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, UpdateItemRequest>> requestBuilderSelector
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new UpdateRequestHandler<TArg, T>(
            new HandlerOptions(),
            requestBuilderSelector(item.ToRequestBuilderFactory()).Build,
            item.Marshaller.Unmarshall
        );
    }

    /// <summary>
    /// Creates a <see cref="PutItemRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<T, T?> ToPutRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<T, PutItemRequest>> requestBuilderSelector
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new PutRequestHandler<T>(
            new HandlerOptions(),
            requestBuilderSelector(item.ToRequestBuilderFactory()).Build,
            item.Marshaller.Unmarshall
        );
    }

    /// <summary>
    /// Creates a <see cref="PutItemRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<T, T?> ToPutRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<T, PutItemRequest>> requestBuilderSelector,
        Action<HandlerOptions> configure
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        var options = new HandlerOptions();
        configure(options);
        return new PutRequestHandler<T>(
            options,
            requestBuilderSelector(item.ToRequestBuilderFactory()).Build,
            item.Marshaller.Unmarshall
        );
    }

    /// <summary>
    /// Creates a <see cref="GetItemRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<TArg, T?> ToGetRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, GetItemRequest>> requestBuilderSelector,
        Action<HandlerOptions> configure
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        var handlerOptions = new HandlerOptions();
        configure(handlerOptions);
        return new GetRequestHandler<TArg, T>(
            handlerOptions,
            requestBuilderSelector(item.ToRequestBuilderFactory()).Build,
            item.Marshaller.Unmarshall
        );
    }


    /// <summary>
    /// Creates a <see cref="GetItemRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<TArg, T?> ToGetRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, GetItemRequest>> requestBuilderSelector
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new GetRequestHandler<TArg, T>(
            new HandlerOptions(),
            requestBuilderSelector(item.ToRequestBuilderFactory()).Build,
            item.Marshaller.Unmarshall
        );
    }


    /// <summary>
    /// Creates a <see cref="DeleteItemRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<TArg, T?> ToDeleteRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, DeleteItemRequest>> requestBuilderSelector
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        return new DeleteRequestHandler<TArg, T>(
            new HandlerOptions(),
            requestBuilderSelector(item.ToRequestBuilderFactory()).Build,
            item.Marshaller.Unmarshall
        );
    }
    
    /// <summary>
    /// Creates a <see cref="DeleteItemRequest"/> based <see cref="IRequestHandler{T, TArg}"/> from an <see cref="IRequestBuilder{TArg, TRequest}"/>.
    /// </summary>
    public static IRequestHandler<TArg, T?> ToDeleteRequestHandler<T, TArg, TReferences, TArgumentReferences>(
        this ITableAccess<T, TArg, TReferences, TArgumentReferences> item,
        Func<IRequestBuilderFactory<T, TArg, TReferences, TArgumentReferences>, IRequestBuilder<TArg, DeleteItemRequest>> requestBuilderSelector,
        Action<HandlerOptions> configure
    )
      where TReferences : IAttributeExpressionNameTracker
      where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
      where TArg : notnull
      where T : notnull
    {
        var options = new HandlerOptions();
        configure(options);
        return new DeleteRequestHandler<TArg, T>(
            options,
            requestBuilderSelector(item.ToRequestBuilderFactory()).Build,
            item.Marshaller.Unmarshall
        );
    }

}
