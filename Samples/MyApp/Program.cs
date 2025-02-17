using Baubit.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyLib;

var hostAppBuilder = new HostApplicationBuilder();
hostAppBuilder.Configuration.AddJsonFile("myConfig.json");
new ServiceProviderFactoryRegistrar().UseDefaultServiceProviderFactory(hostAppBuilder);

var host = hostAppBuilder.Build();
var consumers = host.Services.GetRequiredService<IEnumerable<EventConsumer>>(); //initialize so that they start observing
await host.RunAsync();