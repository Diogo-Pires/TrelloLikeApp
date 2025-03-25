using Confluent.Kafka;
using CuttingEdges.Kafka.Interfaces;
using CuttingEdges.Kafka.Policies;
using Polly.Wrap;
using System.Diagnostics;
using System.Text.Json;

namespace Application.Kafka;

public class KafkaProducerService(KafkaResiliencePolicy resiliencePolicy,
                                  IProducer<string, string> producer) : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer = producer;
    private readonly AsyncPolicyWrap _policy = resiliencePolicy.CreateKafkaPolicy();
    private static readonly ActivitySource ActivitySource = new("KafkaProducer");

    public async System.Threading.Tasks.Task ProduceAsync<T>(string topic, T message, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Processing Kafka Message");

        activity?.SetTag("kafka.message", message);

        await _policy.ExecuteAsync(async () =>
        {
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = JsonSerializer.Serialize(message)
            }, cancellationToken);
        });
    }
}