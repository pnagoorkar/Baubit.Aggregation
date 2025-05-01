using Baubit.Aggregation.Unbounded;
using Baubit.Configuration;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using System.Threading.Channels;

namespace Baubit.Aggregation.DI.Unbounded
{
    public sealed class Module<TEvent> : AModule<Configuration, TEvent, EventAggregator<TEvent>, EventDispatcher<TEvent>>
    {
        public Module(ConfigurationSource configurationSource) : base(configurationSource)
        {
        }

        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        public Module(Configuration configuration, List<AModule> nestedModules, List<IConstraint> constraints) : base(configuration, nestedModules, constraints)
        {
        }

        protected override EventAggregator<TEvent> CreateAggregator(AggregatorConfiguration aggregatorConfiguration, Channel<TEvent> channel, DispatcherFactory<TEvent> dispatcherFactory)
        {
            return new EventAggregator<TEvent>(aggregatorConfiguration, channel, dispatcherFactory);
        }

        protected override Channel<TEvent> CreateChannel()
        {
            return Channel.CreateUnbounded<TEvent>();
        }

        protected override EventDispatcher<TEvent> CreateDispatcher(IObserver<TEvent> observer, IList<AEventDispatcher<TEvent>> dispatchers)
        {
            return new EventDispatcher<TEvent>(observer, dispatchers);
        }
    }
}
