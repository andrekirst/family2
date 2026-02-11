using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public sealed class CircuitBreakerMiddleware(ILogger<CircuitBreakerMiddleware> logger) : IStepMiddleware
{
    private static readonly ConcurrentDictionary<string, CircuitState> Circuits = new();

    private const int FailureThreshold = 5;
    private static readonly TimeSpan RecoveryTimeout = TimeSpan.FromMinutes(1);

    public async Task InvokeAsync(StepPipelineContext context, StepDelegate next, CancellationToken ct)
    {
        var actionType = context.StepExecution.ActionType;
        var state = Circuits.GetOrAdd(actionType, _ => new CircuitState());

        if (state.IsOpen)
        {
            if (DateTime.UtcNow - state.LastFailure > RecoveryTimeout)
            {
                // Half-open: allow one request through to test recovery
                state.IsHalfOpen = true;
                logger.LogInformation("Circuit breaker half-open for {ActionType}", actionType);
            }
            else
            {
                logger.LogWarning(
                    "Circuit breaker open for {ActionType}. Skipping step {StepAlias}",
                    actionType, context.StepExecution.StepAlias);
                context.ShouldSkip = true;
                return;
            }
        }

        try
        {
            await next(context, ct);

            // Success: reset failure count
            state.FailureCount = 0;
            state.IsOpen = false;
            state.IsHalfOpen = false;
        }
        catch (Exception)
        {
            state.FailureCount++;
            state.LastFailure = DateTime.UtcNow;

            if (state.IsHalfOpen || state.FailureCount >= FailureThreshold)
            {
                state.IsOpen = true;
                state.IsHalfOpen = false;
                logger.LogWarning(
                    "Circuit breaker opened for {ActionType} after {FailureCount} failures",
                    actionType, state.FailureCount);
            }

            throw;
        }
    }

    private sealed class CircuitState
    {
        public int FailureCount { get; set; }
        public bool IsOpen { get; set; }
        public bool IsHalfOpen { get; set; }
        public DateTime LastFailure { get; set; }
    }
}
