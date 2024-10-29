using Baubit.Aggregation.ResultReasons;
using FluentResults;
using System.Threading.Channels;
using Baubit.IO;
using Baubit.Traceability;
using Baubit.Aggregation.Traceability;

namespace Baubit.Aggregation
{
    public delegate AEventDispatcher<TEvent> DispatcherFactory<TEvent>(IObserver<TEvent> observer, IList<AEventDispatcher<TEvent>> dispatchers);

    public abstract class AEventDispatcher<TEvent> : IDisposable
    {
        private IObserver<TEvent> _observer;
        private Channel<TEvent> _channel;
        private IList<AEventDispatcher<TEvent>> _dispatchers;
        protected Task _dispatcher;
        protected CancellationTokenSource _instanceCancellationTokenSource = new CancellationTokenSource();

        protected AEventDispatcher(IObserver<TEvent> observer,
                                   IList<AEventDispatcher<TEvent>> dispatchers,
                                   Channel<TEvent> channel)
        {
            _observer = observer;
            _dispatchers = dispatchers;
            _channel = channel;
            _dispatcher = Task.Run(DispatchMessages);
        }

        internal async Task<Result> TryPublish(TEvent @event, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await _channel.TryWriteWhenReadyAsync(@event, cancellationToken, _instanceCancellationTokenSource.Token))
                {
                    return Result.Ok();
                }
                else if (cancellationToken.IsCancellationRequested)
                {
                    return Result.Fail("").WithReason(new CancelledByCaller());
                }
                else if (_instanceCancellationTokenSource.IsCancellationRequested)
                {
                    return Result.Fail("").WithReason(new DispatcherDisposed());
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

        private async Task DispatchMessages()
        {
            await foreach (var @event in _channel.EnumerateAsync(_instanceCancellationTokenSource.Token))
            {
                @event?.CaptureTraceEvent(new OutForDelivery(_observer));
                _observer.OnNext(@event);
                @event?.CaptureTraceEvent(new Delivered(_observer));    
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_instanceCancellationTokenSource.IsCancellationRequested)
            {
                _instanceCancellationTokenSource.Cancel();
                if (disposing)
                {
                    //_dispatcher.Wait(true);
                    _channel.FlushAndDisposeAsync();
                    _dispatchers?.Remove(this);
                    _dispatchers = null;
                }
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
