using Baubit.Configuration;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

namespace Baubit.Aggregation.DI
{
    public abstract class AModule<TConfiguration, TEvent, TAggregator, TDispatcher> : AModule<TConfiguration> where TConfiguration : AConfiguration
                                                                                                              where TAggregator : AEventAggregator<TEvent>
                                                                                                              where TDispatcher : AEventDispatcher<TEvent>
    {

        private Channel<TEvent>? channel;

        protected AModule(ConfigurationSource configurationSource) : base(configurationSource)
        {
        }

        protected AModule(IConfiguration configuration) : base(configuration)
        {
        }

        protected AModule(TConfiguration configuration, List<AModule> nestedModules, List<IConstraint> constraints) : base(configuration, nestedModules, constraints)
        {
        }

        protected override void OnInitialized()
        {
            channel = CreateChannel();
            base.OnInitialized();
        }

        protected override IEnumerable<IConstraint> GetKnownConstraints()
        {
            return [new SingularityConstraint<AModule<TConfiguration, TEvent, TAggregator, TDispatcher>>()];
        }

        public override void Load(IServiceCollection services)
        {
            services.AddSingleton<DispatcherFactory<TEvent>>(CreateDispatcher);
            services.AddSingleton<IEventAggregator<TEvent>>(serviceProvider => CreateAggregator(Configuration.AggregatorConfiguration, channel!, serviceProvider.GetRequiredService<DispatcherFactory<TEvent>>()));
            services.AddSingleton<IObservable<TEvent>>(serviceProvider => serviceProvider.GetRequiredService<IEventAggregator<TEvent>>());
            services.AddSingleton<IEventPublisher, EventPublisher>();
            base.Load(services);
        }

        protected abstract Channel<TEvent> CreateChannel();

        protected abstract TDispatcher CreateDispatcher(IObserver<TEvent> observer, IList<AEventDispatcher<TEvent>> dispatchers);
        protected abstract TAggregator CreateAggregator(AggregatorConfiguration aggregatorConfiguration, Channel<TEvent> channel, DispatcherFactory<TEvent> dispatcherFactory);
    }
}
