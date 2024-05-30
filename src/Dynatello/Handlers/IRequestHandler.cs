namespace Dynatello.Handlers;

public interface IRequestHandler<T, in TArg> : IRequestHandler
  where T : notnull
  where TArg : notnull
{

    public Task<T?> Send(TArg arg, CancellationToken cancellationToken);

    async Task<object?> IRequestHandler.Send(object arg, CancellationToken cancellationToken)
    {
        if (arg is not TArg tArg)
            throw new Exception();

        return await Send(tArg, cancellationToken);
    }

}
public interface IRequestHandler
{
    public Task<object?> Send(object arg, CancellationToken cancellationToken);
}
