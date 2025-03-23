namespace EventProcessing.Middlewares;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) 
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public System.Threading.Tasks.Task HandleExceptionAsync(
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception, "Exception occurred: {Message}", exception.Message);

        return System.Threading.Tasks.Task.CompletedTask;
    }
}