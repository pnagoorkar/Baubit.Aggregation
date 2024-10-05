
using Baubit.Aggregation.Test.AEventAggregator;
using Baubit.Aggregation.Test.Setup;
using Baubit.Configuration;

namespace Baubit.Aggregation.Test.Unbounded
{
    [EmbeddedJsonSources("Baubit.Aggregation.Test;Unbounded.broker.json")]
    public class Broker : ABroker
    {
        public Broker(IEventAggregator<TestEvent> aggregator, IEventPublisher eventPublisher) : base(aggregator, eventPublisher)
        {

        }
    }
}
