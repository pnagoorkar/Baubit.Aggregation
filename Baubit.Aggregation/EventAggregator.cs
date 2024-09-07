//using Baubit.Collections;
//using System.Threading.Channels;

//namespace Baubit.Aggregation
//{
//    /// <summary>
//    /// Implements an event aggregator pattern for non referential event passing in loosely coupled systems
//    /// </summary>
//    /// <typeparam name="TEvent">Type of event</typeparam>
//    public class EventAggregator<TEvent> : IEventAggregator<TEvent>, IDisposable
//    {
//        /// <summary>
//        /// The options this aggregator was initialized with
//        /// </summary>
//        public EventAggregatorOptions Options { get; private init; }

//        private Task distributionFinalizer;
//        private bool disposed = false;

//        private IList<EventDispatcher> _dispatchers;
//        private Channel<TEvent> _channel;
//        private CancellationTokenSource _distributeCancellationTokenSource = new CancellationTokenSource();
//        public EventAggregator(EventAggregatorOptions options = null)
//        {
//            options?.Validate();
//            _dispatchers = new ConcurrentList<EventDispatcher>();
//            Options = options;

//            if (Options != null && Options.AggregationBufferSize != null)
//            {
//                _channel = Channel.CreateBounded<TEvent>(new BoundedChannelOptions(Options.AggregationBufferSize.Value) { FullMode = BoundedChannelFullMode.Wait });
//            }
//            else
//            {
//                _channel = Channel.CreateUnbounded<TEvent>();
//            }

//            distributionFinalizer = Task.Run(DistributeMessages)
//                                        .ContinueWith(FinalizeDispatchers);
//        }

//        /// <summary>
//        /// Tries to enqueue an event in the underlying buffer, to be dispatched to subscribed observers
//        /// </summary>
//        /// <param name="event">The event to publish</param>
//        /// <param name="cancellationToken">A cancellation token that may cancel publication</param>
//        /// <param name="maxWaitToWrite">For a bounded aggregator, this parameter defines the maximum amount of time to wait for the buffer to have space to enquque the event for publication.
//        /// If an explicit value is not passed, <see cref="EventAggregatorOptions.AggregationMaxWaitTime"/> is used as a the max wait time for a bounded aggregator.</param>
//        /// <returns>Result of the publish request</returns>
//        public async Task<EventPublishResult> TryPublishAsync(TEvent @event, CancellationToken cancellationToken = default, TimeSpan? maxWaitToWrite = null)
//        {
//            if (disposed) return EventPublishResults.AggregatorDisposed;
//            await Task.Yield();
//            maxWaitToWrite ??= Options?.AggregationMaxWaitTime;
//            var timeOutCTS = new CancellationTokenSource();
//            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_distributeCancellationTokenSource.Token, cancellationToken, timeOutCTS.Token);
//            var writeAwaiter = _channel.Writer.WaitToWriteAsync(linkedTokenSource.Token);
//            if (maxWaitToWrite == null || maxWaitToWrite == TimeSpan.Zero || maxWaitToWrite == Timeout.InfiniteTimeSpan)
//            {
//                await writeAwaiter;
//            }
//            else if (!writeAwaiter.AsTask().Wait(maxWaitToWrite.Value))
//            {
//                timeOutCTS.Cancel();
//                return EventPublishResults.AggregatorCapacityReached;
//            }
//            var writeSuccess = _channel.Writer.TryWrite(@event);
//            if (writeSuccess)
//            {
//                return EventPublishResults.Successful;
//            }
//            else
//            {
//                if (Options?.AggregationBufferSize != null && _channel.Reader.Count >= Options?.AggregationBufferSize)
//                {
//                    return EventPublishResults.AggregatorCapacityReached;
//                }
//                else
//                {
//                    return EventPublishResults.UnexpectedWriteFailure;
//                }
//            }
//        }

//        /// <summary>
//        /// Registers the <paramref name="observer"/> as a listener for any future events.
//        /// </summary>
//        /// <param name="observer">An interested listener of a <see cref="TEvent"/></param>
//        /// <returns>A disposable subscription that the subscriber must dispose appropriately</returns>
//        public IDisposable Subscribe(IObserver<TEvent> observer)
//        {
//            if (disposed) return null;
//            var dispatcher = new EventDispatcher(observer, _dispatchers, Options);
//            _dispatchers.Add(dispatcher);
//            return dispatcher;
//        }

//        private async Task DistributeMessages()
//        {
//            while (!_distributeCancellationTokenSource.IsCancellationRequested && await _channel.Reader.WaitToReadAsync())
//            {
//                TEvent @event = default;
//                while (!_channel.Reader.TryPeek(out @event)) ;
//                foreach (var dispatcher in _dispatchers)
//                {
//                    EventDispatchResult dispatchResult = null!;
//                    do
//                    {
//                        dispatchResult = await dispatcher.TryPublish(@event, _distributeCancellationTokenSource.Token, Options?.AggregationMaxWaitTime);
//                    } while (!dispatchResult.Success && !_distributeCancellationTokenSource.IsCancellationRequested);
//                }
//                _ = await _channel.Reader.ReadAsync();
//            }
//            if (_distributeCancellationTokenSource.IsCancellationRequested && _channel.Reader.Count > 0)
//            {
//                while (_channel.Reader.TryRead(out _)) ; //empty the channel
//            }
//        }

//        private void FinalizeDispatchers(Task distributor)
//        {
//            foreach (var dispatcher in _dispatchers)
//            {
//                if (distributor.IsFaulted)
//                {
//                    dispatcher.Abort(distributor.Exception);
//                }
//                dispatcher.Dispose();
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }
//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposed)
//            {
//                if (disposing)
//                {
//                    // Dispose managed resources.
//                    if (_channel != null)
//                    {
//                        if (Options?.DisposeWait != null)
//                        {
//                            Thread.Sleep(Options.DisposeWait.Value);
//                        }
//                        else
//                        {
//                            while (_channel.Reader.Count > 0) ;//Allow pending events to be delivered before completing the channel
//                        }
//                        _channel.Writer.Complete();
//                        _distributeCancellationTokenSource.Cancel();
//                    }
//                    distributionFinalizer?.Wait();
//                    _dispatchers?.Clear();
//                    _dispatchers = null;
//                    _channel = null;
//                    distributionFinalizer = null;
//                }

//                // Free unmanaged resources.

//                disposed = true;
//            }
//        }

//        ~EventAggregator()
//        {
//            Dispose(false);
//        }

//        private class EventDispatcher : IDisposable
//        {
//            internal EventAggregatorOptions Options { get; private init; }

//            private Channel<TEvent> _channel;
//            private IObserver<TEvent> _observer;
//            private IList<EventDispatcher> _dispatchers;
//            private Task dispatchFinalizer;
//            private bool disposed = false;
//            private CancellationTokenSource _dispatchCancellationTokenSource = new CancellationTokenSource();
//            internal EventDispatcher(IObserver<TEvent> observer,
//                                     IList<EventDispatcher> dispatchers,
//                                     EventAggregatorOptions? options = null)
//            {
//                _observer = observer;
//                _dispatchers = dispatchers;
//                Options = options;
//                if (Options?.DistributionBufferSize != null)
//                {
//                    _channel = Channel.CreateBounded<TEvent>(new BoundedChannelOptions(Options.DistributionBufferSize.Value) { FullMode = BoundedChannelFullMode.Wait });
//                }
//                else
//                {
//                    _channel = Channel.CreateUnbounded<TEvent>();
//                }

//                dispatchFinalizer = Task.Run(DispatchMessages)
//                                        .ContinueWith(Finalize);
//            }

//            internal async Task<EventDispatchResult> TryPublish(TEvent @event, CancellationToken cancellationToken = default, TimeSpan? maxWaitToWrite = null)
//            {
//                if (disposed) return EventDispatchResults.DispatcherDisposed;
//                await Task.Yield();
//                var timeOutCTS = new CancellationTokenSource();
//                var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_dispatchCancellationTokenSource.Token, cancellationToken, timeOutCTS.Token);
//                var writeAwaiter = _channel.Writer.WaitToWriteAsync(linkedTokenSource.Token);
//                if (maxWaitToWrite == null || maxWaitToWrite == TimeSpan.Zero || maxWaitToWrite == Timeout.InfiniteTimeSpan)
//                {
//                    await writeAwaiter;
//                }
//                else if (!writeAwaiter.AsTask().Wait(maxWaitToWrite.Value))
//                {
//                    timeOutCTS.Cancel();
//                    return EventDispatchResults.DispatcherCapacityReached;
//                }
//                return new EventDispatchResult(_channel.Writer.TryWrite(@event));
//            }

//            internal async Task Abort(Exception exp)
//            {
//                if (disposed) return;
//                _channel.Writer.Complete(exp);
//            }

//            private async Task DispatchMessages()
//            {
//                while (!_dispatchCancellationTokenSource.IsCancellationRequested && await _channel.Reader.WaitToReadAsync())
//                {
//                    TEvent @event = default;
//                    while (!_channel.Reader.TryPeek(out @event)) ;
//                    try
//                    {
//                        _observer.OnNext(@event);
//                    }
//                    catch (Exception exp)
//                    {
//                        _observer.OnError(exp);
//                    }
//                    finally
//                    {
//                        _ = await _channel.Reader.ReadAsync();
//                    }
//                }
//                if (_dispatchCancellationTokenSource.IsCancellationRequested && _channel.Reader.Count > 0)
//                {
//                    while (_channel.Reader.TryRead(out _)) ; //empty the channel
//                }
//            }

//            private void Finalize(Task dispatcher)
//            {
//                if (dispatcher.IsFaulted || _channel.Reader.Completion.IsFaulted)
//                {
//                    _observer.OnError(new AggregateException(dispatcher.Exception, _channel.Reader.Completion.Exception));
//                }
//                else
//                {
//                    _observer.OnCompleted();
//                }
//            }

//            public void Dispose()
//            {
//                Dispose(true);
//                GC.SuppressFinalize(this);
//            }
//            protected virtual void Dispose(bool disposing)
//            {
//                if (!disposed)
//                {
//                    if (disposing)
//                    {
//                        // Dispose managed resources.
//                        if (_channel != null)
//                        {
//                            if (Options?.DisposeWait != null)
//                            {
//                                Thread.Sleep(Options.DisposeWait.Value);
//                            }
//                            else
//                            {
//                                while (_channel.Reader.Count > 0) ;//Allow pending events to be delivered before completing the channel
//                            }
//                            _channel.Writer.Complete();
//                            _dispatchCancellationTokenSource.Cancel();
//                        }
//                        dispatchFinalizer?.Wait();
//                        _dispatchers?.Remove(this);
//                        dispatchFinalizer = null;
//                        _dispatchers = null;
//                        _observer = null;
//                        _channel = null;
//                    }

//                    // Free unmanaged resources.

//                    disposed = true;
//                }
//            }

//            ~EventDispatcher()
//            {
//                Dispose(false);
//            }
//        }
//    }
//}
