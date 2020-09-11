using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JobService.Components;
using MassTransit;
using MassTransit.Conductor;
using MassTransit.Definition;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.JobService;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.EntityFrameworkCoreIntegration.Saga.Context;
using MassTransit.ExtensionsDependencyInjectionIntegration.ScopeProviders;
using MassTransit.JobService.Components.StateMachines;
using MassTransit.JobService.Configuration;
using MassTransit.Registration;
using MassTransit.Saga;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

            services.AddDbContext<JobServiceSagaDbContext>(builder =>
                builder.UseNpgsql(Configuration.GetConnectionString("JobService"), m =>
                {
                    m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                    m.MigrationsHistoryTable($"__{nameof(JobServiceSagaDbContext)}");
                }));

            services.AddMassTransit(x =>
            {
                x.AddRabbitMqMessageScheduler();

                x.AddConsumer<ConvertVideoJobConsumer>(typeof(ConvertVideoJobConsumerDefinition));

                x.AddSagaRepository<JobSaga>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ExistingDbContext<JobServiceSagaDbContext>();
                        r.LockStatementProvider = new PostgresLockStatementProvider();
                    });
                x.AddSagaRepository<JobTypeSaga>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ExistingDbContext<JobServiceSagaDbContext>();
                        r.LockStatementProvider = new PostgresLockStatementProvider();
                    });
                x.AddSagaRepository<JobAttemptSaga>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ExistingDbContext<JobServiceSagaDbContext>();
                        r.LockStatementProvider = new PostgresLockStatementProvider();
                    });

                x.AddServiceClient();

                x.AddRequestClient<ConvertVideo>();

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UseRabbitMqMessageScheduler();

                    var options = new ServiceInstanceOptions()
                        .EnableInstanceEndpoint()
                        .SetEndpointNameFormatter(KebabCaseEndpointNameFormatter.Instance);

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

    public static class JobServiceStartupExtensions
    {
        public static ISagaRegistrationConfigurator<T> AddSagaRepository<T>(this IRegistrationConfigurator configurator) where T : class, ISaga
        {
            if (configurator is RegistrationConfigurator registrationConfigurator)
                return new SagaRegistrationConfigurator<T>(configurator, registrationConfigurator.Registrar);

            throw new ArgumentException("The registrar must be available", nameof(configurator));
        }

        /// <summary>
        /// Configure the job server saga repositories to resolve from the container.
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="provider">The bus registration context provided during configuration</param>
        /// <returns></returns>
        public static IJobServiceConfigurator ConfigureSagaRepositories(this IJobServiceConfigurator configurator, IConfigurationServiceProvider provider)
        {
            configurator.Repository = provider.GetRequiredService<ISagaRepository<JobTypeSaga>>();
            configurator.JobRepository = provider.GetRequiredService<ISagaRepository<JobSaga>>();
            configurator.JobAttemptRepository = provider.GetRequiredService<ISagaRepository<JobAttemptSaga>>();

            return configurator;
        }

        public static void AddSagaRepository<TSaga>(this IServiceCollection services)
            where TSaga : class, ISaga
        {
            var queryExecutor = new PessimisticLoadQueryExecutor<TSaga>(new PostgresLockStatementProvider(), null);

            ISagaRepositoryLockStrategy<TSaga> lockStrategy = new PessimisticSagaRepositoryLockStrategy<TSaga>(queryExecutor, IsolationLevel.Serializable);

            services.AddSingleton(lockStrategy);

            services.AddScoped<ISagaConsumeContextFactory<DbContext, TSaga>, SagaConsumeContextFactory<DbContext, TSaga>>();
            services.AddScoped<ISagaRepositoryContextFactory<TSaga>, EntityFrameworkSagaRepositoryContextFactory<TSaga>>();

            services.AddSingleton<DependencyInjectionSagaRepositoryContextFactory<TSaga>>();
            services.AddSingleton<ISagaRepository<TSaga>>(provider =>
                new SagaRepository<TSaga>(provider.GetRequiredService<DependencyInjectionSagaRepositoryContextFactory<TSaga>>()));

            services.AddScoped<ISagaDbContextFactory<TSaga>, ContainerSagaDbContextFactory<JobServiceSagaDbContext, TSaga>>();
        }
    }
}