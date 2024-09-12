namespace JobService.Components;

using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;


public class MaintenanceConsumer :
    IJobConsumer<PerformRegularlyScheduledMaintenance>
{
    readonly ILogger<MaintenanceConsumer> _logger;

    public MaintenanceConsumer(ILogger<MaintenanceConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Run(JobContext<PerformRegularlyScheduledMaintenance> context)
    {
        _logger.LogInformation("Performing regularly scheduled maintenance");

        await Task.Delay(5000);

        _logger.LogInformation("Maintenance Finished");
    }
}