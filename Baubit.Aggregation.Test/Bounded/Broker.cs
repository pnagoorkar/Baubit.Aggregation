using Baubit.Aggregation.Test.AEventAggregator;
using Baubit.Aggregation.Test.Setup;
using Baubit.xUnit;

namespace Baubit.Aggregation.Test.Bounded
{
    [JsonConfigurationSource("broker")]
    public class Broker : ABroker
    {
        public Broker(IEventAggregator<TestEvent> aggregator, EventAggregatorFactory<TestEvent> aggregatorFactory) : base(aggregator, aggregatorFactory)
        {

        }
    }
}
