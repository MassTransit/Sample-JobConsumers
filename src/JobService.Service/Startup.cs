using System.Linq;
using System.Threading.Tasks;
using JobService.Components;
using MassTransit;
using MassTransit.Conductor;
using MassTransit.Contracts.JobService;
using MassTransit.Definition;
using MassTransit.JobService.Components.StateMachines;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JobService.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddMassTransit(x =>
            {
                x.AddServiceBusMessageScheduler();

                x.AddConsumer<ConvertVideoJobConsumer>(typeof(ConvertVideoJobConsumerDefinition));

                x.AddSagaRepository<JobSaga>()
                    .MessageSessionRepository();
                x.AddSagaRepository<JobTypeSaga>()
                    .MessageSessionRepository();
                x.AddSagaRepository<JobAttemptSaga>()
                    .MessageSessionRepository();

                x.AddServiceClient();

                x.AddRequestClient<ConvertVideo>();

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(Configuration.GetConnectionString("AzureServiceBus"));

                    cfg.Send<ConvertVideo>(t => t.UseSessionIdFormatter(m => m.RequestId?.ToString()));

                    cfg.Send<SetConcurrentJobLimit>(t => t.UseSessionIdFormatter(m => m.Message.JobTypeId.ToString()));
                    cfg.Send<AllocateJobSlot>(t => t.UseSessionIdFormatter(m => m.Message.JobTypeId.ToString()));
                    cfg.Send<JobSlotReleased>(t => t.UseSessionIdFormatter(m => m.Message.JobTypeId.ToString()));

                    cfg.Send<JobSubmitted>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));

                    cfg.Send<JobSlotAllocated>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));
                    cfg.Send<JobSlotUnavailable>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));
                    cfg.Send<Fault<AllocateJobSlot>>(t => t.UseSessionIdFormatter(m => m.Message.Message.JobId.ToString()));

                    cfg.Send<JobSlotWaitElapsed>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));
                    cfg.Send<JobRetryDelayElapsed>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));

                    cfg.Send<JobAttemptCreated>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));
                    cfg.Send<Fault<StartJobAttempt>>(t => t.UseSessionIdFormatter(m => m.Message.Message.AttemptId.ToString()));

                    cfg.Send<StartJobAttempt>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));
                    cfg.Send<Fault<StartJob>>(t => t.UseSessionIdFormatter(m => m.Message.Message.AttemptId.ToString()));

                    cfg.Send<JobAttemptStarted>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));
                    cfg.Send<JobAttemptCompleted>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));
                    cfg.Send<JobAttemptCanceled>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));
                    cfg.Send<JobAttemptFaulted>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));
                    cfg.Send<JobAttemptStatus>(t => t.UseSessionIdFormatter(m => m.Message.JobId.ToString()));

                    cfg.Send<JobStatusCheckRequested>(t => t.UseSessionIdFormatter(m => m.Message.AttemptId.ToString()));

                    cfg.UseServiceBusMessageScheduler();

                    var options = new ServiceInstanceOptions()
                        .EnableInstanceEndpoint()
                        .SetEndpointNameFormatter(KebabCaseEndpointNameFormatter.Instance);

                    // used to set the RequiresSession property on the job service receive endpoints
                    cfg.ConnectEndpointConfigurationObserver(new SessionEndpointConfigurationObserver());

                    cfg.ServiceInstance(options, instance =>
                    {
                        instance.ConfigureJobServiceEndpoints(js =>
                        {
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
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

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