using System;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.EndpointConfigurators;
using MassTransit.Transports.InMemory;

namespace JobService.Service
{
    class SessionEndpointConfigurationObserver :
        IEndpointConfigurationObserver
    {
        public void EndpointConfigured<T>(T configurator)
            where T : IReceiveEndpointConfigurator
        {
            if (configurator.InputAddress.GetQueueOrExchangeName().StartsWith("job", StringComparison.OrdinalIgnoreCase))
                if (configurator is IServiceBusReceiveEndpointConfigurator sb)
                    sb.RequiresSession = true;
        }
    }
}