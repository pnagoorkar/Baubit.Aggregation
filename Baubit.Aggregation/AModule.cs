using Baubit.Configuration;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

namespace Baubit.Aggregation
{
    public abstract class AModule<TConfiguration, TEvent, TAggregator, TDispatcher> : AModule<TConfiguration> where TConfiguration : AModuleConfiguration
                                                                                                              where TAggregator : AEventAggregator<TEvent>
                                                                                                              where TDispatcher : AEventDispatcher<TEvent>
    {
        protected AModule(ConfigurationSource configurationSource) : base(configurationSource)
        {
        }

        protected AModule(IConfiguration configuration) : base(configuration)
        {
        }

        protected AModule(TConfiguration moduleConfiguration, List<AModule> nestedModules) : base(moduleConfiguration, nestedModules)
        {
        }

        private Channel<TEvent>? channel;
        protected override void OnInitialized()
        {
            channel = CreateChannel();
            base.OnInitialized();
        }

        public override void Load(IServiceCollection services)
        {
            services.AddSingleton<DispatcherFactory<TEvent>>(CreateDispatcher);
            services.AddSingleton<IEventAggregator<TEvent>>(serviceProvider => CreateAggregator(channel!, serviceProvider.GetRequiredService<DispatcherFactory<TEvent>>()));
            services.AddSingleton<EventAggregatorFactory<TEvent>>(serviceProvider => serviceProvider.GetRequiredService<IEventAggregator<TEvent>>);
            base.Load(services);
        }

        protected abstract Channel<TEvent> CreateChannel();

        protected abstract TDispatcher CreateDispatcher(IObserver<TEvent> observer, IList<AEventDispatcher<TEvent>> dispatchers);
        protected abstract TAggregator CreateAggregator(Channel<TEvent> channel, DispatcherFactory<TEvent> dispatcherFactory);
    }
}
