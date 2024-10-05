using Baubit.Aggregation.Test.AEventAggregator;
using Baubit.Aggregation.Test.Setup;
using Baubit.xUnit;

namespace Baubit.Aggregation.Test.Bounded
{
    [EmbeddedJsonSources("Baubit.Aggregation.Test;Bounded.broker.json")]
    public class Broker : ABroker
    {
        public Broker(IEventAggregator<TestEvent> aggregator, IEventPublisher eventPublisher) : base(aggregator, eventPublisher)
        {

        }
    }
}
