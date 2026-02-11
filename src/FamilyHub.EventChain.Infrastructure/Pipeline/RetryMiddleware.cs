using Microsoft.Extensions.Logging;

namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public sealed partial class RetryMiddleware(ILogger<RetryMiddleware> logger) : IStepMiddleware
{
    public async Task InvokeAsync(StepPipelineContext context, StepDelegate next, CancellationToken ct)
    {
        var maxRetries = context.StepExecution.MaxRetries;

        while (true)
        {
            try
            {
                await next(context, ct);
                return;
            }
            catch (Exception ex) when (context.StepExecution.CanRetry && !ct.IsCancellationRequested)
            {
                context.StepExecution.IncrementRetry();

                var delay = CalculateBackoffWithJitter(context.StepExecution.RetryCount);

                LogStepStepaliasFailedAttemptAttemptMaxretriesRetryingInDelayMsErrorError(logger, context.StepExecution.StepAlias, context.StepExecution.RetryCount, maxRetries, delay.TotalMilliseconds, ex.Message);

                await Task.Delay(delay, ct);
            }
        }
    }

    private static TimeSpan CalculateBackoffWithJitter(int retryCount)
    {
        var baseDelay = Math.Pow(2, retryCount) * 1000; // Exponential backoff in ms
        var jitter = Random.Shared.Next(0, (int)(baseDelay * 0.3)); // Up to 30% jitter
        return TimeSpan.FromMilliseconds(baseDelay + jitter);
    }

    [LoggerMessage(LogLevel.Warning, "Step {stepAlias} failed (attempt {attempt}/{maxRetries}). Retrying in {delay}ms. Error={error}")]
    static partial void LogStepStepaliasFailedAttemptAttemptMaxretriesRetryingInDelayMsErrorError(ILogger<RetryMiddleware> logger, string stepAlias, int attempt, int maxRetries, double delay, string error);
}
