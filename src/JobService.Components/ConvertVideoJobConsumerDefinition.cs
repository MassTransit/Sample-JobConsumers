namespace JobService.Components;

using System;
using MassTransit;


public class ConvertVideoJobConsumerDefinition :
    ConsumerDefinition<ConvertVideoJobConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ConvertVideoJobConsumer> consumerConfigurator, IRegistrationContext context)
    {
        consumerConfigurator.Options<JobOptions<ConvertVideo>>(options => options
            .SetRetry(r => r.Interval(3, TimeSpan.FromSeconds(30)))
            .SetJobTimeout(TimeSpan.FromMinutes(10))
            .SetConcurrentJobLimit(10)
            .SetJobProperty("DistributionStrategy", "DataCenter")
            .SetInstanceProperty("DataCenter", Environment.GetEnvironmentVariable("DATA_CENTER")));
    }
}