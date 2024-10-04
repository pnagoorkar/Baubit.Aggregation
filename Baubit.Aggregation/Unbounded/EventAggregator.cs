using System.Threading.Channels;

namespace Baubit.Aggregation.Unbounded
{
    public sealed class EventAggregator<TEvent> : AEventAggregator<TEvent>
    {
        public EventAggregator(Channel<TEvent> channel, DispatcherFactory<TEvent> dispatcherFactory) : base(channel, dispatcherFactory)
        {
        }
    }
}
