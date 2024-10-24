using FluentResults;
using System.Threading.Channels;

namespace Baubit.Aggregation.Bounded
{
    public sealed class EventAggregator<TEvent> : AEventAggregator<TEvent>
    {
        private TimeSpan maxWaitToWrite;
        public EventAggregator(BoundedChannelOptions boundedChannelOptions, 
                               DispatcherFactory<TEvent> dispatcherFactory, 
                               TimeSpan maxWaitToWrite) : base(Channel.CreateBounded<TEvent>(boundedChannelOptions), 
                                                                                   dispatcherFactory)
        {
            this.maxWaitToWrite = maxWaitToWrite;
        }
        public override Task<Result> TryPublishAsync(TEvent @event, CancellationToken cancellationToken = default, TimeSpan? maxWaitToWrite = null)
        {
            return base.TryPublishAsync(@event, cancellationToken, maxWaitToWrite ?? this.maxWaitToWrite);
        }
    }
}
