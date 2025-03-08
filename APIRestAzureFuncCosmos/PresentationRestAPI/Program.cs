using PresentationRestAPI.Exceptions;
using Shared.Interfaces;
using Shared;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Shared.Consts;
using Infrastructure.Cache;
using Microsoft.Azure.Cosmos;
using Infrastructure.Config;
using FluentValidation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Text.Json.Serialization;
using PresentationRestAPI.Validators;
using Application.User.Services;
using Application.User.Interfaces;
using Infrastructure.User;
using Infrastructure.Cache.Interfaces;
using Domain.User.Interfaces;
using Domain.Task.Interfaces;
using Application.Task.Interfaces;
using Application.Task.Services;
using Infrastructure.Task;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache(); 
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

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

var cosmosSettings = configuration.GetSection("CosmosDb").Get<CosmosDbSettings>()!;

//To start redis locally, docker run --name redis -p 6379:6379 -d redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = UtilityConsts.APP_NAME;
});

builder.Services.AddSingleton<IHybridCacheService, HybridCacheService>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthorization();
app.MapControllers();
app.Run();
