using Baubit.Aggregation.ResultReasons;
using Baubit.Collections;
using FluentResults;
using System.Threading.Channels;

namespace Baubit.Aggregation
{
    public abstract class AEventAggregator<TEvent> : IEventAggregator<TEvent>
    {
        private IList<AEventDispatcher<TEvent>> _dispatchers = new ConcurrentList<AEventDispatcher<TEvent>>();
        private Channel<TEvent> _channel;
        protected CancellationTokenSource _instanceCancellationTokenSource = new CancellationTokenSource();
        protected Task _distributor;
        private readonly DispatcherFactory<TEvent> _dispatcherFactory;

        protected AEventAggregator(Channel<TEvent> channel, DispatcherFactory<TEvent> dispatcherFactory)
        {
            _channel = channel;
            _dispatcherFactory = dispatcherFactory;
            _distributor = Task.Run(DistributeMessages);
        }

        public IDisposable Subscribe(IObserver<TEvent> observer)
        {
            if (_instanceCancellationTokenSource.IsCancellationRequested) return null;
            var dispatcher = _dispatcherFactory.Invoke(observer, _dispatchers);
            _dispatchers.Add(dispatcher);
            return dispatcher;
        }

        public async Task<Result<IDisposable>> TrySubscribeAsync(IObserver<TEvent> observer)
        {
            await Task.Yield();
            try
            {
                if (_instanceCancellationTokenSource.IsCancellationRequested) return null;
                var dispatcher = _dispatcherFactory.Invoke(observer, _dispatchers);
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
            try
            {
                CancellationTokenSource timedCancellationTokenSource = null;
                if (maxWaitToWrite != null)
                {
                    timedCancellationTokenSource = new CancellationTokenSource(maxWaitToWrite.Value);
                }
                if (await _channel.TryWriteWhenReadyAsync(@event, cancellationToken, _instanceCancellationTokenSource.Token, timedCancellationTokenSource?.Token ?? default))
                {
                    return Result.Ok();
                }
                else if (cancellationToken.IsCancellationRequested)
                {
                    return Result.Fail("").WithReason(new CancelledByCaller());
                }
                else if (_instanceCancellationTokenSource.IsCancellationRequested)
                {
                    return Result.Fail("").WithReason(new AggregatorDisposed());
                }
                else if (timedCancellationTokenSource != null && timedCancellationTokenSource.IsCancellationRequested)
                {
                    return Result.Fail("").WithReason(new WriteTimedOut());
                }
                else
                {
                    return Result.Fail("");
                }
            }
            catch (Exception exp)
            {
                return Result.Fail(new ExceptionalError(exp));
            }
        }

        private async Task DistributeMessages()
        {
            await foreach (var @event in _channel.EnumerateAsync(_instanceCancellationTokenSource.Token))
            {
                foreach (var dispatcher in _dispatchers)
                {
                    var dispatchResult = await dispatcher.TryPublish(@event, _instanceCancellationTokenSource.Token);
                }
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_instanceCancellationTokenSource.IsCancellationRequested)
            {
                _instanceCancellationTokenSource.Cancel();
                if (disposing)
                {
                    _distributor.Wait(true);
                    _channel.FlushAndDisposeAsync();
                    _dispatchers.Dispose();
                }
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
