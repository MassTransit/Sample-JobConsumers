namespace JobService.Service
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Components;
    using JobService.Components;
    using MassTransit;
    using MassTransit.Conductor;
    using MassTransit.Definition;
    using MassTransit.JobService.Components.StateMachines;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;


    public class Startup
    {
        static bool? _isRunningInContainer;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inDocker) && inDocker;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            var storageAccount = CloudStorageAccount.Parse(Configuration.GetConnectionString("AzureTable"));
            var sagaTableClient = storageAccount.CreateCloudTableClient();
            var jobsTable = sagaTableClient.GetTableReference("aaaajobs");
            jobsTable.CreateIfNotExists();
            var jobTypeTable = sagaTableClient.GetTableReference("aaaajobtype");
            jobTypeTable.CreateIfNotExists();
            var jobattemptTable = sagaTableClient.GetTableReference("aaaajobattempt");
            jobattemptTable.CreateIfNotExists();

            services.AddMassTransit(x =>
            {
                x.AddServiceBusMessageScheduler();
                
                x.AddConsumer<ConvertVideoJobConsumer>(typeof(ConvertVideoJobConsumerDefinition));

                x.AddConsumer<VideoConvertedConsumer>();

                x.AddSagaRepository<JobSaga>()
                    .AzureTableRepository(cfg => { cfg.ConnectionFactory(() => jobsTable); });
                x.AddSagaRepository<JobTypeSaga>()
                    .AzureTableRepository(cfg => { cfg.ConnectionFactory(() => jobTypeTable); });
                x.AddSagaRepository<JobAttemptSaga>()
                    .AzureTableRepository(cfg => { cfg.ConnectionFactory(() => jobattemptTable); });

                x.AddServiceClient();

                x.AddRequestClient<ConvertVideo>();

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(Configuration.GetConnectionString("AzureServiceBus"), h =>
                    {
                        h.OperationTimeout = TimeSpan.FromSeconds(60);
                    });

                    cfg.UseServiceBusMessageScheduler();

                    var options = new ServiceInstanceOptions()
                        .EnableInstanceEndpoint()
                        .SetEndpointNameFormatter(KebabCaseEndpointNameFormatter.Instance);

                    cfg.ServiceInstance(options, instance =>
                    {
                        instance.ConfigureJobServiceEndpoints(js =>
                        {
                            js.SagaPartitionCount = 1;
                            js.FinalizeCompleted = true;

                            js.ConfigureSagaRepositories(context);
                        });

                        instance.ConfigureEndpoints(context);
                    });
                });
            });
            services.AddMassTransitHostedService();

            services.AddOpenApiDocument(cfg => cfg.PostProcess = d => d.Info.Title = "Convert Video Service");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseAuthorization();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready"),
                    ResponseWriter = HealthCheckResponseWriter
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions {ResponseWriter = HealthCheckResponseWriter});

                endpoints.MapControllers();
            });
        }

        static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
        {
            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(entry => new JProperty(entry.Key, new JObject(
                    new JProperty("status", entry.Value.Status.ToString()),
                    new JProperty("description", entry.Value.Description),
                    new JProperty("data", JObject.FromObject(entry.Value.Data))))))));

            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(json.ToString(Formatting.Indented));
        }
    }
}