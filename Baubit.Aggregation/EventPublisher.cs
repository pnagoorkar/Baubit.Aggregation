using FluentResults;
using Microsoft.Extensions.DependencyInjection;

namespace Baubit.Aggregation
{
    public sealed class EventPublisher : IEventPublisher
    {
        private IServiceProvider serviceProvider;
        public EventPublisher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public async Task<Result> TryPublishAsync<TEvent>(TEvent @event)
        {
            return await serviceProvider.GetRequiredService<IEventAggregator<TEvent>>().TryPublishAsync(@event);
        }
    }
}

