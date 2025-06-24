using MassTransit;
using MassTransit.RetryPolicies;
using Microsoft.EntityFrameworkCore;

namespace JobService.Service;

public class MigrationHostedService<TDbContext> :
    IHostedService
    where TDbContext : DbContext
{
    readonly ILogger<MigrationHostedService<TDbContext>> _logger;
    readonly IServiceScopeFactory _scopeFactory;

    public MigrationHostedService(IServiceScopeFactory scopeFactory, ILogger<MigrationHostedService<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Retry.Exponential(10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)).Retry(async () =>
        {
            _logger.LogInformation("Applying migrations for {DbContext}", TypeCache<TDbContext>.ShortName);

            await using var scope = _scopeFactory.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

            await context.Database.MigrateAsync(cancellationToken);
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}