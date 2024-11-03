using FluentResults;
using System.Threading.Channels;
using Baubit.IO.Channels;
using Baubit.Tasks;
using Microsoft.Extensions.Hosting;

namespace Baubit.Aggregation
{
    public delegate AEventDispatcher<TEvent> DispatcherFactory<TEvent>(IObserver<TEvent> observer, IList<AEventDispatcher<TEvent>> dispatchers);

    public abstract class AEventDispatcher<TEvent> : IHostedService, IDisposable
    {
        private IObserver<TEvent> _observer;
        private Channel<TEvent> _channel;
        private IList<AEventDispatcher<TEvent>> _dispatchers;
        protected Task _deliveryTask;
        protected CancellationToken _dispatchCancellationToken;

        protected AEventDispatcher(IObserver<TEvent> observer,
                                   IList<AEventDispatcher<TEvent>> dispatchers,
                                   Channel<TEvent> channel)
        {
            _observer = observer;
            _dispatchers = dispatchers;
            _channel = channel;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken == CancellationToken.None || _deliveryTask != null) return Task.CompletedTask;
            _dispatchCancellationToken = cancellationToken;
            _deliveryTask = Task.Run(() => _channel.ReadAsync(OnNextEvent, _dispatchCancellationToken), _dispatchCancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _deliveryTask?.Wait(true);
            _deliveryTask = null;
            return Task.CompletedTask;
        }

        private Task OnNextEvent(TEvent @event, CancellationToken cancellationToken) => _observer.OnNext(@event, cancellationToken);

        internal async Task<Result> TryDispatchAsync(TEvent @event, CancellationToken cancellationToken = default)
        {
            return await _channel.TryWriteWhenReadyAsync(@event, Timeout.InfiniteTimeSpan, cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _deliveryTask?.Wait(true);
                _deliveryTask = null;
                _channel?.FlushAndDispose();
                _channel = null;
                _dispatchers?.Remove(this);
                _dispatchers = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AEventDispatcher()
        {
            Dispose(false);
        }
    }
}
