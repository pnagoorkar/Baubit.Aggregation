using Baubit.Collections;
using FluentResults;
using System.Threading.Channels;
using Baubit.IO.Channels;
using Baubit.Tasks;
using Baubit.Aggregation.ResultReasons;
using Baubit.Traceability;

namespace Baubit.Aggregation
{
    public abstract class AEventAggregator<TEvent> : IEventAggregator<TEvent>
    {
        private IList<AEventDispatcher<TEvent>> _dispatchers = new ConcurrentList<AEventDispatcher<TEvent>>();
        private Channel<TEvent> _channel;
        protected CancellationTokenSource aggregationCancellationTokenSource;
        protected Task _distributor;
        private readonly DispatcherFactory<TEvent> _dispatcherFactory;

        protected AEventAggregator(AggregatorConfiguration aggregatorConfiguration,
                                   Channel<TEvent> channel,
                                   DispatcherFactory<TEvent> dispatcherFactory)
        {
            _channel = channel;
            _dispatcherFactory = dispatcherFactory;
            if (aggregatorConfiguration?.Autostart == true)
            {
                StartAsync(CancellationToken.None);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_distributor != null) return Task.CompletedTask;
            aggregationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Parallel.ForEach(_dispatchers, async dispatcher => await dispatcher.StartAsync(aggregationCancellationTokenSource.Token));
            _distributor = Task.Run(() => _channel.ReadAsync(OnNextEvent, aggregationCancellationTokenSource.Token));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            aggregationCancellationTokenSource.Cancel();
            _distributor?.Wait(true);
            Parallel.ForEach(_dispatchers, async dispatcher => await dispatcher.StopAsync(cancellationToken));
            _distributor = null;
            return Task.CompletedTask;
        }

        public async Task<Result<IDisposable>> TrySubscribeAsync(IObserver<TEvent> observer)
        {
            await Task.Yield();
            try
            {
                if (aggregationCancellationTokenSource != null && aggregationCancellationTokenSource.IsCancellationRequested) return Result.Fail("").WithReason(new AggregatorDisposed());
                var dispatcher = _dispatcherFactory.Invoke(observer, _dispatchers);
                await dispatcher.StartAsync(aggregationCancellationTokenSource?.Token ?? CancellationToken.None);
                _dispatchers.Add(dispatcher);
                return Result.Ok<IDisposable>(dispatcher);
            }
            catch (Exception exp)
            {
                return Result.Fail(new ExceptionalError(exp));
            }
        }

        public async virtual Task<Result> TryPublishAsync(TEvent @event, CancellationToken cancellationToken = default, TimeSpan? maxWaitToWrite = null)
        {
            return await _channel.TryWriteWhenReadyAsync(@event, maxWaitToWrite, cancellationToken);
        }

        private async Task OnNextEvent(TEvent @event, CancellationToken cancellationToken)
        {
            foreach (var dispatcher in _dispatchers)
            {
                var dispatchResult = await dispatcher.TryDispatchAsync(@event, cancellationToken);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            aggregationCancellationTokenSource.Cancel();
            if (disposing)
            {
                _distributor?.Wait(true);
                _distributor = null;
                _channel?.FlushAndDispose();
                _channel = null;
                _dispatchers?.Dispose();
                _dispatchers = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AEventAggregator()
        {
            Dispose(false);
        }
    }
    
}
