using System.Reflection;
using JobService.Sagas;
using JobService.Sagas.Components;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ResQueue;
using ResQueue.Enums;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Fatal)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Fatal)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var connectionString = builder.Configuration.GetConnectionString("JobService");

var serviceBusConnectionString = builder.Configuration.GetConnectionString("AzureServiceBus");
if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
{
    builder.Services.AddOptions<SqlTransportOptions>()
        .Configure(options =>
        {
            options.ConnectionString = connectionString;
        });

    builder.Services.AddPostgresMigrationHostedService();

    builder.Services.AddResQueue(opt =>
    {
        opt.SqlEngine = ResQueueSqlEngine.Postgres;
    });
    builder.Services.AddResQueueMigrationsHostedService();
}


builder.Services.AddDbContext<JobServiceSagaDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(connectionString, m =>
    {
        m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        m.MigrationsHistoryTable($"__{nameof(JobServiceSagaDbContext)}");

        m.EnableRetryOnFailure();
    });
});

builder.Services.AddHostedService<MigrationHostedService<JobServiceSagaDbContext>>();

builder.Services.AddMassTransit(x =>
{
    x.TryAddJobDistributionStrategy<DataCenterJobDistributionStrategy>();

    x.AddJobSagaStateMachines(options => options.FinalizeCompleted = true)
        .SetPartitionedReceiveMode()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<JobServiceSagaDbContext>();
            r.UsePostgres();
        });

    x.SetKebabCaseEndpointNameFormatter();

    if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
    {
        x.AddSqlMessageScheduler();

        x.UsingPostgres((context, cfg) =>
        {
            cfg.UseSqlMessageScheduler();
            cfg.UseJobSagaPartitionKeyFormatters();

            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        x.AddServiceBusMessageScheduler();

        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(serviceBusConnectionString);

            cfg.UseServiceBusMessageScheduler();
            cfg.UseJobSagaPartitionKeyFormatters();

            cfg.ConfigureEndpoints(context);
        });
    }
});

builder.Services.AddOptions<MassTransitHostOptions>()
    .Configure(options =>
    {
        options.WaitUntilStarted = true;
        options.StartTimeout = TimeSpan.FromMinutes(1);
        options.StopTimeout = TimeSpan.FromMinutes(1);
    });

builder.Services.AddOptions<HostOptions>()
    .Configure(options => options.ShutdownTimeout = TimeSpan.FromMinutes(1));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseRouting();
app.UseAuthorization();

if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
    app.UseResQueue();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter
});

app.MapHealthChecks("/health/live", new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter });

app.MapControllers();

await app.RunAsync();

static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
{
    context.Response.ContentType = "application/json";

    return context.Response.WriteAsync(result.ToJsonString());
}