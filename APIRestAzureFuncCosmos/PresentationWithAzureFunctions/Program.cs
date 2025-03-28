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
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PresentationWithAzureFunctions.Validators;
using Shared;
using Shared.Consts;
using Shared.Interfaces;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

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

var configuration = builder.Configuration;

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

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = UtilityConsts.APP_NAME;
});

builder.Services.AddSingleton<IHybridCacheService, HybridCacheService>();
builder.Services.AddSingleton(x => new CosmosClient(cosmosSettings?.Endpoint, cosmosSettings?.Key));
builder.Services.AddSingleton(x => cosmosSettings!);
builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddTransient<ITaskRepository, TaskRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ITaskService, TaskService>();
builder.Services.AddTransient<IUserService, UserService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateTaskValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

builder.Build().Run();
