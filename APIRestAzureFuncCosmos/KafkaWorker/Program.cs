using EventProcessing.Mail.Interfaces;
using EventProcessing.Mail;
using EventProcessing.Mail.Settings;
using EventProcessing.Middlewares;
using CuttingEdges.Kafka.Policies;
using EventProcessing.Mail.Policies;
using CuttingEdges.Kafka.Settings;
using EventProcessing.Task;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<GlobalExceptionHandler>();
builder.Services.AddSingleton<KafkaResiliencePolicy>();
builder.Services.AddSingleton<EmailResiliencePolicy>();
builder.Services.AddSingleton<IEmailService, EmailService>();

builder.Services.AddHostedService<TaskAssignedKafkaConsumer>();

var host = builder.Build();
host.Run();