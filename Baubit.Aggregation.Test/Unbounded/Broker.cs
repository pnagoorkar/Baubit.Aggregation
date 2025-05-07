
using Baubit.Aggregation.Test.AEventAggregator;
using Baubit.Aggregation.Test.Setup;
using Baubit.Reflection;

namespace Baubit.Aggregation.Test.Unbounded
{
    [Source(EmbeddedJsonResources = ["Baubit.Aggregation.Test;Unbounded.broker.json"])]
    public class Broker : AContext
    {
        public Broker(IEventAggregator<TestEvent> aggregator, IEventPublisher eventPublisher) : base(aggregator, eventPublisher)
        {

        }
    }
}
