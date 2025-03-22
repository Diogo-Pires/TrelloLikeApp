using Confluent.Kafka;
using Shared.Task.Model;
using System.Text.Json;

namespace KafkaWorker;

public class KafkaConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;

    public KafkaConsumer()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "email-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe("task-assigned");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                var message = JsonSerializer.Deserialize<TaskAssignedMessage>(consumeResult.Value);

                _consumer.Commit();
            }
            catch (Exception ex)
            {
            }
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
