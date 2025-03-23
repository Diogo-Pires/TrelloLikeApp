using EventProcessing.Kafka;
using EventProcessing.Mail.Interfaces;
using EventProcessing.Mail;
using EventProcessing.Mail.Settings;
using EventProcessing.Middlewares;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<GlobalExceptionHandler>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddHostedService<KafkaConsumerService>();

var host = builder.Build();
host.Run();