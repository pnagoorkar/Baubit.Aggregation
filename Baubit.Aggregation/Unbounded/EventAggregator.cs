using System.Threading.Channels;

namespace Baubit.Aggregation.Unbounded
{
    public sealed class EventAggregator<TEvent> : AEventAggregator<TEvent>
    {
        public EventAggregator(DispatcherFactory<TEvent> dispatcherFactory) : base(Channel.CreateUnbounded<TEvent>(), dispatcherFactory)
        {
        }
    }
}
