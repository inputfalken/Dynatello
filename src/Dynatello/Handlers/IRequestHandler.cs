namespace Dynatello.Handlers;

/// <summary>
/// 
/// </summary>
public interface IRequestHandler<in TArg, T>
{
    /// <summary>
    /// 
    /// </summary>
    public Task<T> Send(TArg arg, CancellationToken cancellationToken);
}

