namespace Baubit.Aggregation
{
    public delegate Task<EventPublishResult?> TryPublishAsync<TEvent>(TEvent @event);
}
