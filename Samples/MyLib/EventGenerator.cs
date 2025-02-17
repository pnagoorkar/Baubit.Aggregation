using Baubit.Aggregation;
using Microsoft.Extensions.Hosting;

namespace MyLib
{
    public class EventGenerator : IHostedService
    {
        private IEventAggregator<Event> _eventAggregator;
        public EventGenerator(IEventAggregator<Event> eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 10; i++)
            {
                var result = await _eventAggregator.TryPublishAsync(new Event { EventId = i });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
