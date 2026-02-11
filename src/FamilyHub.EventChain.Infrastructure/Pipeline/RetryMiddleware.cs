using Microsoft.Extensions.Logging;

namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public sealed class RetryMiddleware(ILogger<RetryMiddleware> logger) : IStepMiddleware
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

                logger.LogWarning(
                    "Step {StepAlias} failed (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms. Error={Error}",
                    context.StepExecution.StepAlias,
                    context.StepExecution.RetryCount,
                    maxRetries,
                    delay.TotalMilliseconds,
                    ex.Message);

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
}
