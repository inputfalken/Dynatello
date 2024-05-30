namespace Dynatello.Handlers;

public interface ITaskHandler<T, in TArg> : ITaskHandler
  where T : notnull
  where TArg : notnull
{

    public Task<T?> Send(TArg arg, CancellationToken cancellationToken);

    async Task<object?> ITaskHandler.Send(object arg, CancellationToken cancellationToken)
    {
        if (arg is not TArg tArg)
            throw new Exception();

        return await Send(tArg, cancellationToken);
    }

}
public interface ITaskHandler
{
    public Task<object?> Send(object arg, CancellationToken cancellationToken);
}
