using Polly.Wrap;
using Polly;

namespace EventProcessing.Mail.Policies;

public class EmailResiliencePolicy(ILogger<EmailResiliencePolicy> logger)
{
    private readonly ILogger<EmailResiliencePolicy> _logger = logger;

    public AsyncPolicyWrap CreateEmailPolicy()
    {
        //Retries mail sending operations up to 5 times using exponential backoff
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"Retry {retryCount} after {timeSpan.TotalSeconds} seconds due to {exception.Message}");
                });

        //Opens after 3 failures, preventing excessive retries for 30 seconds
        var circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, timespan) =>
                {
                    _logger.LogError($"Email Circuit broken for {timespan.TotalSeconds} seconds due to {exception.Message}");
                },
                onReset: () =>
                {
                    _logger.LogInformation("Email Circuit reset. Email operations will resume.");
                });

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }
}
