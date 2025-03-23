namespace Shared.Settings;

public record KafkaSettings
{
    public required string Url { get; set; }
    public required int TimeoutMs { get; set; }
    public required int SessionTimeoutMs { get; set; }
    public required int StatisticsIntervalMs { get; set; }
    public required string TaskAssignedTopic { get; set; }
    public required string TaskConsumerGroup { get; set; }
}
