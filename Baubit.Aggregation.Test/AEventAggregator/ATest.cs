using Baubit.xUnit;
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

        [Fact(DisplayName = "All events are delivered to all consumers"), Order("a")]
        public virtual async Task Test_EventDelivery()
        {
            Broker.ResetEvents();
            foreach (var @event in Broker.Events)
            {
                EventPublishResult result = null;
                do
                {
                    result = await Broker.Aggregator.TryPublishAsync(@event);
                    if (result.Success)
                    {
                        @event.PostedAt = DateTime.UtcNow;
                    }
                } while (!result.Success);
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
        public void AggregatorFactoryIsResolved()
        {
            Assert.NotNull(Broker.AggregatorFactory);
            Assert.NotNull(Broker.AggregatorFactory());
        }

        [Fact(DisplayName = "Cannot publish event after aggregator disposed"), Order("z")]
        public virtual async Task Test_AggregatorDisposal()
        {
            Broker.ResetEvents();
            Broker.Aggregator.Dispose();
            var result = await Broker.Aggregator.TryPublishAsync(Broker.Events.First());
            Assert.False(result.Success);
        }
    }
}
