using Baubit.Configuration;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyLib
{
    public class MyModule : AModule<MyModuleConfiguration>
    {
        public MyModule(ConfigurationSource configurationSource) : base(configurationSource)
        {
        }

        public MyModule(IConfiguration configuration) : base(configuration)
        {
        }

        public MyModule(MyModuleConfiguration configuration, List<AModule> nestedModules, List<IConstraint> constraints) : base(configuration, nestedModules, constraints)
        {
        }

        public override void Load(IServiceCollection services)
        {
            int consumerCount = Random.Shared.Next(5, 10);
            for (int i = 0; i < consumerCount; i++)
            {
                services.AddScoped(serviceProvider => new EventConsumer(serviceProvider.GetRequiredService<Baubit.Aggregation.IObservable<Event>>(),
                                                                        serviceProvider.GetRequiredService<ILogger<EventConsumer>>()));
            }
            services.AddHostedService<EventGenerator>();
            base.Load(services);
        }
    }
}
