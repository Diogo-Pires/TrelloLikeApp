using Confluent.Kafka;
using CuttingEdges.Kafka.Interfaces;
using CuttingEdges.Kafka.Policies;
using CuttingEdges.Kafka.Settings;
using Microsoft.Extensions.Options;
using Polly.Wrap;
using System.Diagnostics;
using System.Text.Json;

namespace Application.Kafka;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private readonly AsyncPolicyWrap _policy;
    private static readonly ActivitySource ActivitySource = new("KafkaProducer");

    public KafkaProducerService(IOptions<KafkaSettings> options, KafkaResiliencePolicy resiliencePolicy)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.Url,
            Acks = Acks.All, 
            MessageTimeoutMs = options.Value.TimeoutMs
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
        _policy = resiliencePolicy.CreateKafkaPolicy();
    }

    public async System.Threading.Tasks.Task ProduceAsync<T>(string topic, T message)
    {
        using var activity = ActivitySource.StartActivity("Processing Kafka Message");

        activity?.SetTag("kafka.message", message);

        await _policy.ExecuteAsync(async () =>
        {
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = JsonSerializer.Serialize(message)
            });
        });
    }
}