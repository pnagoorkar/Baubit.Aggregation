using Baubit.Aggregation.Bounded;
using Baubit.Aggregation.Test.AEventAggregator;
using Baubit.Aggregation.Test.Setup;
using Baubit.xUnit;
using System.Threading.Channels;

namespace Baubit.Aggregation.Test.Bounded
{
    [JsonConfigurationSource("broker")]
    public class Broker : ABroker
    {

        static ModuleConfiguration moduleConfiguration = new ModuleConfiguration { Capacity = 10, FullMode = BoundedChannelFullMode.Wait, MaxWaitToWriteMS = 10 };
        static BoundedChannelOptions boundedChannelOptions = new BoundedChannelOptions(moduleConfiguration.Capacity) { FullMode = moduleConfiguration.FullMode };
        public Broker() : base(GenerateEventAggregator())
        {

        }

        private static EventAggregator<TestEvent> GenerateEventAggregator()
        {
            var maxWaitToWrite = new TimeSpan(0, 0, 0, 0, moduleConfiguration.MaxWaitToWriteMS);

            var eventAggregator = new EventAggregator<TestEvent>(boundedChannelOptions, CreateBoundedEventDispatcher, maxWaitToWrite);
            return eventAggregator;
        }

        private static AEventDispatcher<TEvent> CreateBoundedEventDispatcher<TEvent>(IObserver<TEvent> observer, IList<AEventDispatcher<TEvent>> dispatchers)
        {
            return new EventDispatcher<TEvent>(observer, dispatchers, boundedChannelOptions);
        }
    }
}
