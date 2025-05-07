using Baubit.Aggregation.Test.AEventAggregator;
using Baubit.Aggregation.Test.Setup;
using Baubit.Reflection;

namespace Baubit.Aggregation.Test.Bounded
{
    [Source(EmbeddedJsonResources = ["Baubit.Aggregation.Test;Bounded.broker.json"])]
    public class Context : AContext
    {
        public Context(IEventAggregator<TestEvent> aggregator, IEventPublisher eventPublisher) : base(aggregator, eventPublisher)
        {

        }
    }
}
