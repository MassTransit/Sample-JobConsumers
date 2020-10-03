using System;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using MassTransit.JobService;
using Microsoft.Extensions.Logging;

namespace JobService.Components
{
    public interface IConvertVideo
    {
        string Path { get; }
    }

    public class ConvertVideoJobConsumer : IJobConsumer<IConvertVideo>
    {
        readonly ILogger<ConvertVideoJobConsumer> _logger;

        public ConvertVideoJobConsumer(ILogger<ConvertVideoJobConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Run(JobContext<IConvertVideo> context)
        {
            _logger.LogInformation("Converting video: {Path}", context.Job.Path);

            await Task.Delay(TimeSpan.FromSeconds(15));

            _logger.LogInformation("Converted video: {Path}", context.Job.Path);
        }
    }

    public class ConvertVideoJobConsumerDefinition : ConsumerDefinition<ConvertVideoJobConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<ConvertVideoJobConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<JobOptions<IConvertVideo>>(options =>
                options
                    .SetRetry(r => r.Interval(3, TimeSpan.FromSeconds(30)))
                    .SetJobTimeout(TimeSpan.FromMinutes(10))
                    .SetConcurrentJobLimit(10));
        }
    }
}