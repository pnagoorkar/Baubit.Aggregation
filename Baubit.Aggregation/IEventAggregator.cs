using FluentResults;
using Microsoft.Extensions.Hosting;

namespace Baubit.Aggregation
{
    /// <summary>
    /// Provides methods for non referential event passing
    /// </summary>
    /// <typeparam name="TEvent">Type of event</typeparam>
    public interface IEventAggregator<TEvent> : IObservable<TEvent>, IHostedService, IDisposable
    {
        /// <summary>
        /// Tries to enqueue an event in the underlying buffer, to be dispatched to subscribed observers
        /// </summary>
        /// <param name="event">The event to publish</param>
        /// <param name="cancellationToken">A cancellation token that may cancel publication</param>
        /// <param name="maxWaitToWrite">For a bounded aggregator, this parameter defines the maximum amount of time to wait for the buffer to have space to enquque the event for publication.
        /// If an explicit value is not passed, <see cref="EventAggregatorOptions.AggregationMaxWaitTime"/> is used as a the max wait time for a bounded aggregator.</param>
        /// <returns>Result of the publish request</returns>
        Task<Result> TryPublishAsync(TEvent @event, CancellationToken cancellationToken = default, TimeSpan? maxWaitToWrite = null);
    }
}
