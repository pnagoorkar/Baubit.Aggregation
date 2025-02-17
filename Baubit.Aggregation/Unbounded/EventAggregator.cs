using System.Threading.Channels;

namespace Baubit.Aggregation.Unbounded
{
    public sealed class EventAggregator<TEvent> : AEventAggregator<TEvent>
    {
        public EventAggregator(AggregatorConfiguration aggregatorConfiguration,
                               Channel<TEvent> channel, 
                               DispatcherFactory<TEvent> dispatcherFactory) : base(aggregatorConfiguration, channel, dispatcherFactory)
        {
        }
    }
}
