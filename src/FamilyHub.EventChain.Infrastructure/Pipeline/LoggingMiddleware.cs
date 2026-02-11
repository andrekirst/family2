using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public sealed partial class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IStepMiddleware
{
    public async Task InvokeAsync(StepPipelineContext context, StepDelegate next, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        LogStepStepaliasActiontypeStartingCorrelationidCorrelationidChainexecutionidChainexecutionid(logger, context.StepExecution.StepAlias, context.StepExecution.ActionType, context.CorrelationId, context.ChainExecution.Id.Value);

        try
        {
            await next(context, ct);
            sw.Stop();

            if (context.ShouldSkip)
            {
                LogStepStepaliasSkippedConditionNotMetDurationDurationMs(logger, context.StepExecution.StepAlias, sw.ElapsedMilliseconds);
            }
            else
            {
                LogStepStepaliasCompletedSuccessfullyDurationDurationMs(logger, context.StepExecution.StepAlias, sw.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            LogStepStepaliasFailedAfterDurationMsErrorError(logger, context.StepExecution.StepAlias, sw.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }

    [LoggerMessage(LogLevel.Information, "Step {stepAlias} ({actionType}) starting. CorrelationId={correlationId}, ChainExecutionId={chainExecutionId}")]
    static partial void LogStepStepaliasActiontypeStartingCorrelationidCorrelationidChainexecutionidChainexecutionid(ILogger<LoggingMiddleware> logger, string stepAlias, string actionType, Guid correlationId, Guid chainExecutionId);

    [LoggerMessage(LogLevel.Information, "Step {stepAlias} skipped (condition not met). Duration={duration}ms")]
    static partial void LogStepStepaliasSkippedConditionNotMetDurationDurationMs(ILogger<LoggingMiddleware> logger, string stepAlias, long duration);

    [LoggerMessage(LogLevel.Information, "Step {stepAlias} completed successfully. Duration={duration}ms")]
    static partial void LogStepStepaliasCompletedSuccessfullyDurationDurationMs(ILogger<LoggingMiddleware> logger, string stepAlias, long duration);

    [LoggerMessage(LogLevel.Error, "Step {stepAlias} failed after {duration}ms. Error={error}")]
    static partial void LogStepStepaliasFailedAfterDurationMsErrorError(ILogger<LoggingMiddleware> logger, string stepAlias, long duration, string error);
}
