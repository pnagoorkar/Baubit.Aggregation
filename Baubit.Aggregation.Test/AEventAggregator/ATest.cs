﻿using Baubit.Aggregation.ResultReasons;
using Baubit.Aggregation.Traceability;
using Baubit.xUnit;
using FluentResults;
using Xunit.Abstractions;

namespace Baubit.Aggregation.Test.AEventAggregator
{
    public abstract class ATest<TBroker> : AClassFixture<Fixture<TBroker>, TBroker> where TBroker : ABroker
    {
        protected ATest(Fixture<TBroker> fixture, ITestOutputHelper testOutputHelper, IMessageSink diagnosticMessageSink = null) : base(fixture, testOutputHelper, diagnosticMessageSink)
        {
            if (Broker.Events.Count == 0)
            {
                Broker.GenerateEvents(10000);
            }
            if (Broker.Consumers.Count == 0)
            {
                Broker.GenerateConsumers(100);
            }
        }

        [Fact(DisplayName = "All events are delivered to all consumers")]
        [Order("a")]
        public virtual async Task Test_EventDelivery()
        {
            Broker.ResetEvents();
            foreach (var @event in Broker.Events)
            {
                Result result = null;
                do
                {
                    result = await Broker.Aggregator.TryPublishAsync(@event);
                    if (result.IsSuccess)
                    {
                        @event.PostedAt = DateTime.UtcNow;
                    }
                } while (!result.IsSuccess);
            }
            var expectedNumOfReceipts = Broker.Consumers.Count * Broker.Events.Count;
            var actualNumOfReceipts = Broker.Events.Sum(@event => @event.Trace.Count);
            while (actualNumOfReceipts < expectedNumOfReceipts)
            {
                Thread.Sleep(1);
                actualNumOfReceipts = Broker.Events.Sum(@event => @event.Trace.Count);
            }
            Assert.Equal(expectedNumOfReceipts, actualNumOfReceipts);
        }
        [Fact]
        [Order("aa")]
        public void EventPublisherIsResolved()
        {
            Assert.NotNull(Broker.EventPublisher);
        }

        [Fact]
        [Order("aaa")]
        public async Task EventsAreTraceable()
        {
            Broker.ResetEvents();
            foreach (var @event in Broker.Events)
            {
                @event.EnableTrace = true;
                Result result = null;
                do
                {
                    result = await Broker.Aggregator.TryPublishAsync(@event);
                    if (result.IsSuccess)
                    {
                        @event.PostedAt = DateTime.UtcNow;
                    }
                } while (!result.IsSuccess);
            }

            var expectedNumOfReceipts = Broker.Consumers.Count * Broker.Events.Count;
            var actualNumOfReceipts = Broker.Events.Sum(@event => @event.Trace.Count);
            while (actualNumOfReceipts < expectedNumOfReceipts)
            {
                Thread.Sleep(1);
                actualNumOfReceipts = Broker.Events.Sum(@event => @event.Trace.Count);
            }

            var mergedHistories = Broker.Events.SelectMany(evt => evt.History);

            var receivedCount = mergedHistories.Count(evt => evt is Received);
            var dispatchCancelledCount = mergedHistories.Count(evt => evt is DispatchCancelled);
            var outForDispatchCount = mergedHistories.Count(evt => evt is OutForDispatch);
            var dispatchedCount = mergedHistories.Count(evt => evt is Dispatched);
            var outForDeliveryCount = mergedHistories.Count(evt => evt is OutForDelivery);
            var deliveredCount = mergedHistories.Count(evt => evt is Delivered);

            var dispatchCancelledDueToTimeout = mergedHistories.Count(evt => evt.Reasons.Any(reason => reason is WriteTimedOut));

            Assert.Equal(expectedNumOfReceipts, actualNumOfReceipts);
            Assert.Equal(receivedCount - dispatchCancelledCount, Broker.Events.Count);
            Assert.Equal(outForDispatchCount, Broker.Events.Count);
            Assert.Equal(outForDispatchCount, Broker.Events.Count);
            Assert.Equal(outForDeliveryCount, actualNumOfReceipts);
            Assert.Equal(deliveredCount, actualNumOfReceipts);
            Assert.Equal(dispatchCancelledCount, dispatchCancelledDueToTimeout);
            Parallel.ForEach(Broker.Events, @event => @event.EnableTrace = false);
        }

        [Fact(DisplayName = "Cannot publish event after aggregator disposed"), Order("z")]
        public virtual async Task Test_AggregatorDisposal()
        {
            Broker.ResetEvents();
            Broker.Aggregator.Dispose();
            var result = await Broker.Aggregator.TryPublishAsync(Broker.Events.First());
            Assert.False(result.IsSuccess);
        }
    }
}
