namespace JobService.Components
{
    using System;
    using System.Threading.Tasks;
    using MassTransit.JobService;
    using Microsoft.Extensions.Logging;


    public class ConvertVideoJobConsumer :
        IJobConsumer<ConvertVideo>
    {
        readonly ILogger<ConvertVideoJobConsumer> _logger;

        public ConvertVideoJobConsumer(ILogger<ConvertVideoJobConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Run(JobContext<ConvertVideo> context)
        {
            _logger.LogInformation("Converting video: {Path}", context.Job.Path);

            await Task.Delay(TimeSpan.FromSeconds(15));

            _logger.LogInformation("Converted video: {Path}", context.Job.Path);
        }
    }
}