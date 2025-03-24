using CuttingEdges.Kafka.Policies;
using CuttingEdges.Kafka.Settings;
using EventProcessing.Kafka;
using EventProcessing.Mail.Interfaces;
using EventProcessing.Mail.Policies;
using EventProcessing.Middlewares;
using Microsoft.Extensions.Options;
using Polly.Wrap;
using Shared.Task.Model;
using System.Text.Json;

namespace EventProcessing.Task;

public class TaskAssignedKafkaConsumer(IOptions<KafkaSettings> options,
                                       ILogger<KafkaConsumerService> baseLogger,
                                       ILogger<TaskAssignedKafkaConsumer> logger,
                                       IEmailService emailService,
                                       KafkaResiliencePolicy kafkaResiliencePolicy,
                                       EmailResiliencePolicy emailResiliencepolicy,
                                       GlobalExceptionHandler exceptionHandler) : 
    KafkaConsumerService(options.Value.TaskAssignedTopic, 
                         options.Value.TaskConsumerGroup, 
                         options,
                         baseLogger,
                         kafkaResiliencePolicy,
                         exceptionHandler)
{
    private readonly ILogger<TaskAssignedKafkaConsumer> _logger = logger;
    private readonly IEmailService emailService = emailService;
    private readonly AsyncPolicyWrap _emailPolicy = emailResiliencepolicy.CreateEmailPolicy();

    protected override async System.Threading.Tasks.Task ProcessMessageAsync(string rawMessage, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<TaskAssignedMessage>(rawMessage);
        if(message == null)
        {
            _logger.LogError($"Message was not in the correct format: {rawMessage}");
            return; 
        }

        await _emailPolicy.ExecuteAsync(async () =>
        {
            await emailService.SendEmailAsync(message.Email, "test", "body", cancellationToken);
        });
    }
}
