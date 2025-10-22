namespace JobService.Service;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using JobService.Components;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


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
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        
        await using var scope = _scopeFactory.CreateAsyncScope();

        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        await endpoint.AddOrUpdateRecurringJob(nameof(MaintenanceConsumer), new PerformRegularlyScheduledMaintenance(), x => x.Every(minutes: 1),
            stoppingToken);
    }
}