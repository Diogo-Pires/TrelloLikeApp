using Application.Kafka.Interfaces;
using Application.Kafka.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Application.Kafka;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducerService(IOptions<KafkaSettings> options)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.Url
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async System.Threading.Tasks.Task ProduceAsync<T>(string topic, T message)
    {
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = JsonSerializer.Serialize(message)
        });
    }
}