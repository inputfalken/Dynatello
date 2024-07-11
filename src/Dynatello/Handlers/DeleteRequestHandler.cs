namespace Dynatello.Handlers;

internal sealed class DeleteREquestHandler<TArg, T> : IRequestHandler<TArg, T>
{
    public Task<T> Send(TArg arg, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

