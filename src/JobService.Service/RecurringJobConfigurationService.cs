using JobService.Components;
using MassTransit;

namespace JobService.Service;

public class RecurringJobConfigurationService :
    BackgroundService
{
    readonly IServiceScopeFactory _scopeFactory;

    public RecurringJobConfigurationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        await endpoint.AddOrUpdateRecurringJob(nameof(MaintenanceConsumer), new PerformRegularlyScheduledMaintenance(), x => x.Every(minutes: 1),
            stoppingToken);
    }
}