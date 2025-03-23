using Confluent.Kafka;
using EventProcessing.Middlewares;
using Microsoft.Extensions.Options;
using Shared.Settings;
using Shared.Task.Model;
using System.Text.Json;

namespace EventProcessing.Kafka;

public abstract class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly GlobalExceptionHandler _exceptionHandler;

    public KafkaConsumerService(string topic,
                                string consumerGroup,
                                IOptions<KafkaSettings> options,
                                ILogger<KafkaConsumerService> logger,
                                GlobalExceptionHandler exceptionHandler)
    {
        _logger = logger;
        _exceptionHandler = exceptionHandler;

        var config = new ConsumerConfig
        {
            BootstrapServers = options.Value.Url,
            GroupId = consumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false, 
            SessionTimeoutMs = options.Value.SessionTimeoutMs,
            StatisticsIntervalMs = options.Value.StatisticsIntervalMs
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((c, e) => _logger.LogError("Kafka Consumer Error: {Error}", e.Reason))
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                _logger.LogInformation("Assigned partitions: {Partitions}", string.Join(", ", partitions));
            })
            .SetPartitionsRevokedHandler((c, partitions) =>
            {
                _logger.LogWarning("Revoked partitions: {Partitions}", string.Join(", ", partitions));
            })
            .Build();

        _consumer.Subscribe(topic);
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(cancellationToken);
                if (consumeResult == null &&
                   consumeResult?.Message == null &&
                   consumeResult?.Message.Value == null)
                {
                    continue;
                }

                await ProcessMessageAsync(consumeResult.Message.Value, cancellationToken);

                _consumer.Commit();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Kafka consumer stopping due to cancellation request.");
            }
            catch (Exception ex)
            {
                await _exceptionHandler.HandleExceptionAsync(ex, cancellationToken);
            }
        }
    }

    protected abstract System.Threading.Tasks.Task ProcessMessageAsync(string message, CancellationToken cancellationToken);

    public override async System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Close();
        _consumer.Dispose();
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}