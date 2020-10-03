using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using JobService.Components;

using MassTransit;
using MassTransit.Conductor;
using MassTransit.Definition;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.JobService;
using MassTransit.JobService.Components.StateMachines;

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
            {
                builder.UseSqlServer(Configuration.GetConnectionString(nameof(JobServiceSagaDbContext)), options =>
                {
                    options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                });
            });


            //services.AddMassTransit(x =>
            //{

            //    x.AddRabbitMqMessageScheduler();

            //    x.AddConsumer<ConvertVideoJobConsumer>();
            //    // x.AddConsumer<ConvertVideoJobConsumer>(typeof(ConvertVideoJobConsumerDefinition));

            //    x.AddSagaRepository<JobSaga>()
            //        .EntityFrameworkRepository(r =>
            //        {
            //            r.ExistingDbContext<JobServiceSagaDbContext>();
            //            r.LockStatementProvider = new PostgresLockStatementProvider();
            //        });
            //    x.AddSagaRepository<JobTypeSaga>()
            //        .EntityFrameworkRepository(r =>
            //        {
            //            r.ExistingDbContext<JobServiceSagaDbContext>();
            //            r.LockStatementProvider = new PostgresLockStatementProvider();
            //        });
            //    x.AddSagaRepository<JobAttemptSaga>()
            //        .EntityFrameworkRepository(r =>
            //        {
            //            r.ExistingDbContext<JobServiceSagaDbContext>();
            //            r.LockStatementProvider = new PostgresLockStatementProvider();
            //        });

            //    x.AddServiceClient();

            //    x.AddRequestClient<IConvertVideo>();

            //    x.SetKebabCaseEndpointNameFormatter();

            //    x.UsingRabbitMq((context, cfg) =>
            //    {
            //        var rabbit = Configuration.GetSection("RabbitServer");
            //        var url = rabbit.GetValue<string>("Url");
            //        var host = rabbit.GetValue<string>("Host");
            //        var userName = rabbit.GetValue<string>("UserName");
            //        var password = rabbit.GetValue<string>("Password");
            //        if (rabbit == null || url == null || host == null || userName == null || password == null)
            //        {
            //            throw new InvalidOperationException("RabbitService configuration settings are not found in appSettings.json");
            //        }

            //        cfg.Host($"rabbitmq://{url}/{host}", configurator =>
            //        {
            //            configurator.Username(userName);
            //            configurator.Password(password);
            //        });

            //        cfg.UseRabbitMqMessageScheduler();

            //        var options = new ServiceInstanceOptions()
            //            .EnableInstanceEndpoint()
            //            .SetEndpointNameFormatter(KebabCaseEndpointNameFormatter.Instance);

            //        cfg.ServiceInstance(options, instance =>
            //        {
            //            instance.ConfigureJobServiceEndpoints(js =>
            //            {
            //                js.FinalizeCompleted = true;

            //                js.ConfigureSagaRepositories(context);
            //            });

            //            instance.ConfigureEndpoints(context);
            //        });
            //    });
            //});


            //services.AddOpenApiDocument(cfg => cfg.PostProcess = d => d.Info.Title = "Convert Video Service");

            services.AddMassTransit(x =>
            {




                x.AddRabbitMqMessageScheduler();
                x.AddConsumer<ConvertVideoJobConsumer>();
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

                x.AddRequestClient<IConvertVideo>();

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {

                    var options = new ServiceInstanceOptions().EnableInstanceEndpoint();

                    cfg.ServiceInstance(options, instance =>
                    {
                        var rabbit = Configuration.GetSection("RabbitServer");
                        var url = rabbit.GetValue<string>("Url");
                        var host = rabbit.GetValue<string>("Host");
                        var userName = rabbit.GetValue<string>("UserName");
                        var password = rabbit.GetValue<string>("Password");
                        if (rabbit == null || url == null || host == null || userName == null || password == null)
                        {
                            throw new InvalidOperationException("RabbitService configuration settings are not found in appSettings.json");
                        }

                        cfg.Host($"rabbitmq://{url}/{host}", configurator =>
                        {
                            configurator.Username(userName);
                            configurator.Password(password);
                        });

                        //cfg.ConfigureEndpoints(busFactory, SnakeCaseEndpointNameFormatter.Instance);
                        //cfg.UseJsonSerializer();
                        //cfg.UseHealthCheck(busFactory);

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

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter });

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