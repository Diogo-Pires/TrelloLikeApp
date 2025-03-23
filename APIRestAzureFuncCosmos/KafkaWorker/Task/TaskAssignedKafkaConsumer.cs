using EventProcessing.Kafka;
using EventProcessing.Mail.Interfaces;
using EventProcessing.Middlewares;
using Microsoft.Extensions.Options;
using Shared.Settings;
using Shared.Task.Model;
using System.Text.Json;

namespace EventProcessing.Task;

public class TaskAssignedKafkaConsumer(IOptions<KafkaSettings> options,
                                       ILogger<KafkaConsumerService> baseLogger,
                                       ILogger<TaskAssignedKafkaConsumer> logger,
                                       IEmailService emailService,
                                       GlobalExceptionHandler exceptionHandler) : 
    KafkaConsumerService(options.Value.TaskAssignedTopic, 
                         options.Value.TaskConsumerGroup, 
                         options,
                         baseLogger, 
                         exceptionHandler)
{
    private readonly ILogger<TaskAssignedKafkaConsumer> _logger = logger;
    private readonly IEmailService emailService = emailService;

    protected override async System.Threading.Tasks.Task ProcessMessageAsync(string rawMessage, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<TaskAssignedMessage>(rawMessage);
        if(message == null)
        {
            return; //TBD: Deal with errors
        }

        await emailService.SendEmailAsync(message.Email, "test", "body", cancellationToken);
    }
}
