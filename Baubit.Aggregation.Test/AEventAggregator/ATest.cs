using Baubit.xUnit;
using FluentResults;
using Xunit.Abstractions;

namespace Baubit.Aggregation.Test.AEventAggregator
{
    public abstract class ATest<TBroker> : AClassFixture<Fixture<TBroker>, TBroker> where TBroker : AContext
    {
        protected ATest(Fixture<TBroker> fixture, ITestOutputHelper testOutputHelper, IMessageSink diagnosticMessageSink = null) : base(fixture, testOutputHelper, diagnosticMessageSink)
        {
            if (Context.Events.Count == 0)
            {
                Context.GenerateEvents(10000);
            }
            if (Context.Consumers.Count == 0)
            {
                Context.GenerateConsumers(100);
            }
        }

        [Fact(DisplayName = "All events are delivered to all consumers")]
        [Order("a")]
        public virtual async Task Test_EventDelivery()
        {
            Context.ResetEvents();
            foreach (var @event in Context.Events)
            {
                Result result = null;
                do
                {
                    result = await Context.Aggregator.TryPublishAsync(@event);
                    if (result.IsSuccess)
                    {
                        @event.PostedAt = DateTime.UtcNow;
                    }
                } while (!result.IsSuccess);
            }
            var expectedNumOfReceipts = Context.Consumers.Count * Context.Events.Count;
            var actualNumOfReceipts = Context.Events.Sum(@event => @event.Trace.Count);
            while (actualNumOfReceipts < expectedNumOfReceipts)
            {
                Thread.Sleep(1);
                actualNumOfReceipts = Context.Events.Sum(@event => @event.Trace.Count);
            }
            Assert.Equal(expectedNumOfReceipts, actualNumOfReceipts);
        }
        [Fact]
        [Order("aa")]
        public void EventPublisherIsResolved()
        {
            Assert.NotNull(Context.EventPublisher);
        }

        [Fact(DisplayName = "Cannot publish event after aggregator disposed"), Order("z")]
        public virtual async Task Test_AggregatorDisposal()
        {
            Context.ResetEvents();
            Context.Aggregator.Dispose();
            var result = await Context.Aggregator.TryPublishAsync(Context.Events.First());
            Assert.False(result.IsSuccess);
        }
    }
}
