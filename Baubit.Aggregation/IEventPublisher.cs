namespace Baubit.Aggregation
{
    public interface IEventPublisher
    {
        Task<EventPublishResult?> TryPublishAsync<TEvent>(TEvent @event);
    }
}
