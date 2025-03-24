using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;

namespace CuttingEdges.Kafka.Policies;

public class KafkaResiliencePolicy(ILogger<KafkaResiliencePolicy> logger)
{
    private readonly ILogger<KafkaResiliencePolicy> _logger = logger;

    public AsyncPolicyWrap CreateKafkaPolicy()
    {
        //Retries Kafka operations up to 5 times using exponential backoff
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"Kafka retry {retryCount} after {timeSpan.TotalSeconds} seconds due to {exception.Message}");
                });

        //Opens after 3 failures, preventing excessive retries for 30 seconds
        var circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, timespan) =>
                {
                    _logger.LogError($"Circuit broken for {timespan.TotalSeconds} seconds due to {exception.Message}");
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit reset. Kafka operations will resume.");
                });

        //Logs errors and prevents crashes if retries and circuit breaker fail
        var fallbackPolicy = Policy
            .Handle<Exception>()
            .FallbackAsync(
                fallbackAction: async ct =>
                {
                    _logger.LogError("Kafka operation failed. Executing fallback logic.");
                    await Task.CompletedTask;
                });

        return Policy.WrapAsync(fallbackPolicy, retryPolicy, circuitBreakerPolicy);
    }
}