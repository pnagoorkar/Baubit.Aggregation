using FluentResults;

namespace Baubit.Aggregation
{
    public interface IObservable<TEvent>
    {
        Task<Result<IDisposable>> TrySubscribeAsync(IObserver<TEvent> observer);
    }
}
