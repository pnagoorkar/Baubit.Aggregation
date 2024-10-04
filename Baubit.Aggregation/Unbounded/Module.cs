﻿using Baubit.Configuration;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using System.Threading.Channels;

namespace Baubit.Aggregation.Unbounded
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

        protected override EventAggregator<TEvent> CreateAggregator(Channel<TEvent> channel, DispatcherFactory<TEvent> dispatcherFactory)
        {
            return new EventAggregator<TEvent>(channel, dispatcherFactory);
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
