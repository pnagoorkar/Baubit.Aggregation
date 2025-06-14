﻿using Baubit.Aggregation.Test.Setup;
using Baubit.Testing;

namespace Baubit.Aggregation.Test.AEventAggregator
{
    public abstract class AContext : IContext
    {
        public List<EventConsumer> Consumers { get; set; }
        public List<TestEvent> Events { get; set; }
        public IEventAggregator<TestEvent> Aggregator { get; set; }
        public IEventPublisher EventPublisher { get; set; }

        protected AContext(IEventAggregator<TestEvent> aggregator, IEventPublisher eventPublisher)
        {
            Aggregator = aggregator;
            Aggregator.StartAsync(CancellationToken.None);
            EventPublisher = eventPublisher;
            Events = new List<Setup.TestEvent>();
            Consumers = new List<Setup.EventConsumer>();
        }

        public void GenerateEvents(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Events.Add(new TestEvent());
            }
        }

        public void ResetEvents()
        {
            Events.ForEach(@event => { @event.PostedAt = default; @event.Trace.Clear(); });
        }

        public void GenerateConsumers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Consumers.Add(new Setup.EventConsumer(Aggregator));
            }
        }
        public void RemoveAllConsumers()
        {
            Consumers.ForEach(consumer => consumer.Dispose());
            Consumers.Clear();
        }
        public void GenerateBusyConsumers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Consumers.Add(new Setup.BusyConsumer(Aggregator));
            }
        }
    }
}
