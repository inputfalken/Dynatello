namespace Dynatello.Handlers;

/// <summary>
/// 
/// </summary>
public interface IRequestHandler<T, in TArg>
  where T : notnull
  where TArg : notnull
{
    /// <summary>
    /// 
    /// </summary>
    public Task<T?> Send(TArg arg, CancellationToken cancellationToken);
}

/// <summary>
/// 
/// </summary>
public interface IRequestHandler<T>
{
    /// <summary>
    /// 
    /// </summary>
    public Task Send(T arg, CancellationToken cancellationToken);
}
