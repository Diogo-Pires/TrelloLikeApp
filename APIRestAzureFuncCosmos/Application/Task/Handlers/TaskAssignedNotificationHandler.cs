using Application.Task.Commands;
using CuttingEdges.Kafka.Interfaces;
using CuttingEdges.Kafka.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Task.Handlers;

public class TaskAssignedNotificationHandler(
    IKafkaProducerService kafkaProducerService, 
    IOptions<KafkaSettings> options) : IRequestHandler<TaskAssignedNotificationCommand>
{
    private readonly IKafkaProducerService _kafkaProducerService = kafkaProducerService;
    private readonly KafkaSettings _kafkaOptions = options.Value;

    public async System.Threading.Tasks.Task Handle(TaskAssignedNotificationCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return;
        }
           
        await _kafkaProducerService.ProduceAsync(_kafkaOptions.TaskAssignedTopic, request, cancellationToken);
    } 
}