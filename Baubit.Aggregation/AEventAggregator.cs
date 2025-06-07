using Baubit.Collections;
using FluentResults;
using System.Threading.Channels;
using Baubit.IO.Channels;
using Baubit.Tasks;
using Baubit.Aggregation.ResultReasons;
using Baubit.Traceability;
using FluentResults.Extensions;
using System.Collections.Concurrent;

namespace Baubit.Aggregation
{
    public enum AggregatorState
    {
        None,
        NotStarted,
        Running,
        Faulted,
        Stopped,
        Disposing,
        Disposed
    }
    public abstract class AEventAggregator<TEvent> : IEventAggregator<TEvent>
    {
        private IList<AEventDispatcher<TEvent>> _dispatchers = new ConcurrentList<AEventDispatcher<TEvent>>();
        private Channel<TEvent> _channel;
        protected CancellationTokenSource aggregationCancellationTokenSource;
        protected Task _distributor;
        private readonly DispatcherFactory<TEvent> _dispatcherFactory;

        private ConcurrentStack<AggregatorState> aggregatorStates = new ConcurrentStack<AggregatorState>([AggregatorState.NotStarted]);

        public AggregatorState State 
        {
            get
            {
                return aggregatorStates.TryPeek(out var currentState) ? currentState : AggregatorState.None;
            }
            private set
            {
                aggregatorStates.Push(value);
            }
        }

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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            (await StartAsyncInternal(cancellationToken)).ThrowIfFailed();
        }

        private async Task<Result> StartAsyncInternal(CancellationToken cancellationToken)
        {
            return await Result.OkIf(_distributor == null, string.Empty)
                               .Bind(() => Result.Try(() => aggregationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)))
                               .Bind(_ => Result.Try(() => Parallel.ForEach(_dispatchers, async dispatcher => await dispatcher.StartAsync(aggregationCancellationTokenSource.Token))))
                               .Bind(parallelLoopResult => Result.OkIf(parallelLoopResult.IsCompleted, string.Empty))
                               .Bind(() => Result.Try(() => _distributor = Task.Run(() => _channel.ReadAsync(OnNextEvent, aggregationCancellationTokenSource.Token))))
                               .Bind(() => Result.Try(() => State = AggregatorState.Running).Bind(_ => Task.FromResult(Result.Ok())));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            (await StopAsyncInternal(cancellationToken)).ThrowIfFailed();
        }

        private async Task<Result> StopAsyncInternal(CancellationToken cancellationToken)
        {
            return await Result.Try(() => aggregationCancellationTokenSource.Cancel())
                               .Bind(() => _distributor?.Wait(true))
                               .Bind(() => Result.Try(() => Parallel.ForEach(_dispatchers, async dispatcher => await dispatcher.StopAsync(cancellationToken))))
                               .Bind(parallelLoopResult => Result.OkIf(parallelLoopResult.IsCompleted, string.Empty))
                               .Bind(() => Result.Try(() => _distributor = null))
                               .Bind(() => Result.Try(() => State = AggregatorState.Stopped).Bind(_ => Task.FromResult(Result.Ok())));
        }

        public async Task<Result<IDisposable>> TrySubscribeAsync(IObserver<TEvent> observer)
        {
            return await Result.FailIf(aggregationCancellationTokenSource?.IsCancellationRequested == true, string.Empty)
                         .AddReasonIfFailed(new AggregatorDisposed())
                         .Bind(() => Result.Try(() => _dispatcherFactory.Invoke(observer, _dispatchers)))
                         .Bind(dispatcher => Result.Try(() => dispatcher.StartAsync(aggregationCancellationTokenSource?.Token ?? CancellationToken.None))
                                                   .Bind(() => Task.FromResult(Result.Try(() => _dispatchers.Add(dispatcher)).Bind(() => Result.Ok<IDisposable>(dispatcher)))));
        }

        public async virtual Task<Result> TryPublishAsync(TEvent @event, CancellationToken cancellationToken = default, TimeSpan? maxWaitToWrite = null)
        {
            return await _channel.TryWriteWhenReadyAsync(@event, maxWaitToWrite, cancellationToken);
        }

        private async Task OnNextEvent(TEvent @event, CancellationToken cancellationToken)
        {
            await _dispatchers.Aggregate(Task.FromResult(Result.Ok()), (seed, next) => seed.Bind(() => next.TryDispatchAsync(@event, cancellationToken)));
        }

        protected virtual void Dispose(bool disposing)
        {
            aggregationCancellationTokenSource.Cancel();
            if (disposing)
            {
                State = AggregatorState.Disposing;
                _distributor?.Wait(true);
                _distributor = null;
                _channel?.FlushAndDispose();
                _channel = null;
                _dispatchers?.Dispose();
                _dispatchers = null;
                State = AggregatorState.Disposed;
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
