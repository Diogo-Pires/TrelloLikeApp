using Application.Kafka.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Shared.Settings;
using System.Diagnostics;
using System.Text.Json;

namespace Application.Kafka;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private static readonly ActivitySource ActivitySource = new("KafkaProducer");

    public KafkaProducerService(IOptions<KafkaSettings> options)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.Url,
            Acks = Acks.All, 
            MessageTimeoutMs = options.Value.TimeoutMs
        };

        _producer = new ProducerBuilder<string, string>(config)
            .Build();
    }

    public async System.Threading.Tasks.Task ProduceAsync<T>(string topic, T message)
    {
        using var activity = ActivitySource.StartActivity("Processing Kafka Message");

        activity?.SetTag("kafka.message", message);
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = JsonSerializer.Serialize(message)
        });
    }
}