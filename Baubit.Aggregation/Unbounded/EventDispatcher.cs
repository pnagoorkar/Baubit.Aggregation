
using System.Threading.Channels;

namespace Baubit.Aggregation.Unbounded
{
    public sealed class EventDispatcher<TEvent> : AEventDispatcher<TEvent>
    {
        public EventDispatcher(IObserver<TEvent> observer, IList<AEventDispatcher<TEvent>> dispatchers) : base(observer, dispatchers, Channel.CreateUnbounded<TEvent>())
        {
        }
    }
}
