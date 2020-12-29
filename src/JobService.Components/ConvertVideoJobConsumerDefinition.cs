namespace JobService.Components
{
    using System;
    using GreenPipes;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;
    using MassTransit.JobService;


    public class ConvertVideoJobConsumerDefinition :
        ConsumerDefinition<ConvertVideoJobConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<ConvertVideoJobConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<JobOptions<ConvertVideo>>(options =>
                options.SetRetry(r => r.Interval(3, TimeSpan.FromSeconds(30))).SetJobTimeout(TimeSpan.FromMinutes(10)).SetConcurrentJobLimit(10));
        }
    }
}