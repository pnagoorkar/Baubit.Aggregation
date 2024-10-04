
using System.Threading.Channels;

namespace Baubit.Aggregation.Bounded
{

    public sealed class EventDispatcher<TEvent> : AEventDispatcher<TEvent>
    {
        public EventDispatcher(IObserver<TEvent> observer, 
                               IList<AEventDispatcher<TEvent>> dispatchers, 
                               BoundedChannelOptions boundedChannelOptions) : base(observer, dispatchers, Channel.CreateBounded<TEvent>(boundedChannelOptions))
        {
        }
    }
}
