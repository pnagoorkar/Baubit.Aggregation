using FluentResults;

namespace Baubit.Aggregation
{
    public interface IEventPublisher
    {
        Task<Result> TryPublishAsync<TEvent>(TEvent @event);
    }
}
