using Baubit.Aggregation.Bounded;
using Baubit.Configuration;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using System.Threading.Channels;

namespace Baubit.Aggregation.DI.Bounded
{
    public sealed class Module<TEvent> : AModule<Configuration, TEvent, EventAggregator<TEvent>, EventDispatcher<TEvent>>
    {

        BoundedChannelOptions? boundedChannelOptions;

        public Module(ConfigurationSource configurationSource) : base(configurationSource)
        {
        }

        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        public Module(Configuration configuration, List<AModule> nestedModules, List<IConstraint> constraints) : base(configuration, nestedModules, constraints)
        {
        }

        protected override void OnInitialized()
        {
            boundedChannelOptions = new BoundedChannelOptions(Configuration.Capacity) { FullMode = Configuration.FullMode };
            base.OnInitialized();
        }

        protected override Channel<TEvent> CreateChannel()
        {
            return Channel.CreateBounded<TEvent>(boundedChannelOptions!);
        }

        protected override EventDispatcher<TEvent> CreateDispatcher(IObserver<TEvent> observer,
                                                                    IList<AEventDispatcher<TEvent>> dispatchers)
        {
            return new EventDispatcher<TEvent>(observer, dispatchers, boundedChannelOptions!);
        }

        protected override EventAggregator<TEvent> CreateAggregator(AggregatorConfiguration aggregatorConfiguration, Channel<TEvent> channel, DispatcherFactory<TEvent> dispatcherFactory)
        {
            return new EventAggregator<TEvent>(aggregatorConfiguration, boundedChannelOptions!, dispatcherFactory, new TimeSpan(Configuration.MaxWaitToWriteMS));
        }
    }
}
