using Baubit.Configuration;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using System.Threading.Channels;

namespace Baubit.Aggregation.Bounded
{
    public sealed class Module<TEvent> : AModule<ModuleConfiguration, TEvent, EventAggregator<TEvent>, EventDispatcher<TEvent>>
    {
        public Module(ConfigurationSource configurationSource) : base(configurationSource)
        {
        }

        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        public Module(ModuleConfiguration moduleConfiguration, List<AModule> nestedModules) : base(moduleConfiguration, nestedModules)
        {
        }

        BoundedChannelOptions? boundedChannelOptions;
        protected override void OnInitialized()
        {
            boundedChannelOptions = new BoundedChannelOptions(ModuleConfiguration.Capacity) { FullMode = ModuleConfiguration.FullMode };
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

        protected override EventAggregator<TEvent> CreateAggregator(Channel<TEvent> channel, DispatcherFactory<TEvent> dispatcherFactory)
        {
            return new EventAggregator<TEvent>(boundedChannelOptions!, dispatcherFactory, new TimeSpan(ModuleConfiguration.MaxWaitToWriteMS));
        }
    }
}
