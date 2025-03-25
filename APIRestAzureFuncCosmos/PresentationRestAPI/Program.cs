using PresentationRestAPI.Exceptions;
using Shared.Interfaces;
using Shared;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Shared.Consts;
using Infrastructure.Cache;
using Microsoft.Azure.Cosmos;
using Infrastructure.Config;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Text.Json.Serialization;
using Application.User.Services;
using Application.User.Interfaces;
using Infrastructure.User;
using Infrastructure.Cache.Interfaces;
using Domain.User.Interfaces;
using Domain.Task.Interfaces;
using Application.Task.Interfaces;
using Application.Task.Services;
using Infrastructure.Task;
using PresentationRestAPI.Task.Validators;
using PresentationRestAPI.User.Validators;
using PresentationRestAPI.Task.Interfaces;
using PresentationRestAPI.User.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PresentationRestAPI.User.Middleswares;
using System.Threading.RateLimiting;
using Asp.Versioning;
using PresentationRestAPI;
using Asp.Versioning.ApiExplorer;
using Application.Kafka;
using Application.Task.Handlers;
using CuttingEdges.Kafka.Policies;
using CuttingEdges.Kafka.Interfaces;
using CuttingEdges.Kafka.Settings;
using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);
var corsPolicyName = "AllowFrontend";

var configuration = builder.Configuration;

//Rate Limiter
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5, 
                Window = TimeSpan.FromSeconds(10), 
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }
        );
    });

    options.RejectionStatusCode = 429;
});

//Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),  
        new HeaderApiVersionReader("X-Api-Version"), 
        new QueryStringApiVersionReader("api-version") 
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VV"; 
    options.SubstituteApiVersionInUrl = true;
});

//Regular stuff
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddMemoryCache();

//CORS
var corsSettings = configuration.GetSection("CORS:FrontendURLs").Get<List<string>>()!;
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, builder =>
    {
        builder.WithOrigins([.. corsSettings])
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

//Authentication and authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = "https://accounts.google.com";
        options.MetadataAddress = "https://accounts.google.com/.well-known/openid-configuration";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://accounts.google.com",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Google:ClientId"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

//Helpers
builder.Services.AddHealthChecks();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

//Observability
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

builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(UtilityConsts.APP_NAME))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(UtilityConsts.APP_NAME)
            .AddConsoleExporter()
            .AddOtlpExporter()
        )
        .WithTracing(tracing => tracing.AddSource("KafkaProducer"))
        //.WithMetrics(metrics => metrics.AddMeter("KafkaMetrics"))
        .WithMetrics(metrics => metrics
            .AddMeter(UtilityConsts.APP_NAME)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
        );


//Redis
var redisUrl = configuration.GetSection("Redis:Url").Get<string>()!;
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisUrl;
    options.InstanceName = UtilityConsts.APP_NAME;
});

var cosmosSettings = configuration.GetSection("CosmosDb").Get<CosmosDbSettings>()!;
var kafkaSection = builder.Configuration.GetSection("Kafka");
var kafkaSettings = kafkaSection.Get<KafkaSettings>();

//Configuration
builder.Services.Configure<KafkaSettings>(kafkaSection);

//DIs
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TaskAssignedNotificationHandler).Assembly));
builder.Services.AddSingleton(
    c => new ProducerBuilder<string, string>(
        new ProducerConfig
        {
            BootstrapServers = kafkaSettings?.Url,
            Acks = Acks.All,
            MessageTimeoutMs = kafkaSettings?.TimeoutMs
        }).Build()
);
builder.Services.AddSingleton<KafkaResiliencePolicy>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<IHybridCacheService, HybridCacheService>();
builder.Services.AddSingleton(x => new CosmosClient(cosmosSettings.Endpoint, cosmosSettings.Key));
builder.Services.AddSingleton(x => cosmosSettings);
builder.Services.AddScoped<ITaskCreateValidator, TaskCreateValidator>();
builder.Services.AddScoped<ITaskUpdateValidator, TaskUpdateValidator>();
builder.Services.AddScoped<IUserCreatorValidator, UserCreateValidator>();
builder.Services.AddTransient<ITaskRepository, TaskRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ITaskService, TaskService>();
builder.Services.AddTransient<IUserService, UserService>();


var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseRouting();
app.UseRateLimiter();
app.UseCors(corsPolicyName);
app.UseAuthentication(); 
app.UseMiddleware<UserValidationMiddleware>(); 
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant()); 
        } 
    });
}
app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.UseExceptionHandler();
app.MapControllers();
app.Run();
