using Baubit.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyLib;

var host = Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory()
          .Build();

var consumers = host.Services.GetRequiredService<IEnumerable<EventConsumer>>(); //initialize so that they start observing

await host.StartAsync();
