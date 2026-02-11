using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public sealed partial class CircuitBreakerMiddleware(ILogger<CircuitBreakerMiddleware> logger) : IStepMiddleware
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
                LogCircuitBreakerHalfOpenForActiontype(logger, actionType);
            }
            else
            {
                LogCircuitBreakerOpenForActiontypeSkippingStepStepalias(logger, actionType, context.StepExecution.StepAlias);
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
                LogCircuitBreakerOpenedForActiontypeAfterFailurecountFailures(logger, actionType, state.FailureCount);
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

    [LoggerMessage(LogLevel.Information, "Circuit breaker half-open for {actionType}")]
    static partial void LogCircuitBreakerHalfOpenForActiontype(ILogger<CircuitBreakerMiddleware> logger, string actionType);

    [LoggerMessage(LogLevel.Warning, "Circuit breaker open for {actionType}. Skipping step {stepAlias}")]
    static partial void LogCircuitBreakerOpenForActiontypeSkippingStepStepalias(ILogger<CircuitBreakerMiddleware> logger, string actionType, string stepAlias);

    [LoggerMessage(LogLevel.Warning, "Circuit breaker opened for {actionType} after {failureCount} failures")]
    static partial void LogCircuitBreakerOpenedForActiontypeAfterFailurecountFailures(ILogger<CircuitBreakerMiddleware> logger, string actionType, int failureCount);
}
