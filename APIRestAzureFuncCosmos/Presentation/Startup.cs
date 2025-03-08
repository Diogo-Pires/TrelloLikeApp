using Application.Task.Interfaces;
using Application.Task.Services;
using Application.User.Interfaces;
using Application.User.Services;
using Domain.Task.Interfaces;
using Domain.User.Interfaces;
using FluentValidation;
using Infrastructure.Cache;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Config;
using Infrastructure.Task;
using Infrastructure.User;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Presentation.Exceptions;
using Presentation.Interfaces;
using Presentation.Validators;
using Shared;
using Shared.Consts;
using Shared.Interfaces;

[assembly: FunctionsStartup(typeof(Presentation.Startup))]
namespace Presentation;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging(logging =>
        {
            logging.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(UtilityConsts.APP_NAME));
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.AddConsoleExporter();
            });
        });

        var configuration = builder.GetContext().Configuration;

        ////To start jaeger locally, docker run --name jaeger -p 16686:16686 -p 4317:4317 -p 4318:4318 -p 6831:6831/udp jaegertracing/all-in-one:latest
        builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(UtilityConsts.APP_NAME))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(UtilityConsts.APP_NAME)
                    .AddConsoleExporter()
                    .AddOtlpExporter()
                )
                .WithMetrics(metrics => metrics
                    .AddMeter(UtilityConsts.APP_NAME)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter()
                );

        var cosmosSettings = configuration.GetSection("CosmosDb").Get<CosmosDbSettings>();

        //To start redis locally, docker run --name redis -p 6379:6379 -d redis
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = "localhost:6379";
            options.InstanceName = UtilityConsts.APP_NAME;
        });

        builder.Services.AddSingleton<IHybridCacheService, HybridCacheService>();
        builder.Services.AddSingleton<IExceptionHandler, GlobalExceptionHandler>();
        builder.Services.AddSingleton(x => new CosmosClient(cosmosSettings.Endpoint, cosmosSettings.Key));
        builder.Services.AddSingleton(x => cosmosSettings);
        builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddTransient<ITaskRepository, TaskRepository>();
        builder.Services.AddTransient<IUserRepository, UserRepository>();
        builder.Services.AddTransient<ITaskService, TaskService>();
        builder.Services.AddTransient<IUserService, UserService>();

        builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<UpdateTaskValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
    }
}