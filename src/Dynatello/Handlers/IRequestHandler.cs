namespace Dynatello.Handlers;

/// <summary>
/// 
/// </summary>
public interface IRequestHandler<T, in TArg>
{
    /// <summary>
    /// 
    /// </summary>
    public Task<T> Send(TArg arg, CancellationToken cancellationToken);
}
