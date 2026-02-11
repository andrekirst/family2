using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public sealed class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IStepMiddleware
{
    public async Task InvokeAsync(StepPipelineContext context, StepDelegate next, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        logger.LogInformation(
            "Step {StepAlias} ({ActionType}) starting. CorrelationId={CorrelationId}, ChainExecutionId={ChainExecutionId}",
            context.StepExecution.StepAlias,
            context.StepExecution.ActionType,
            context.CorrelationId,
            context.ChainExecution.Id.Value);

        try
        {
            await next(context, ct);
            sw.Stop();

            if (context.ShouldSkip)
            {
                logger.LogInformation(
                    "Step {StepAlias} skipped (condition not met). Duration={Duration}ms",
                    context.StepExecution.StepAlias,
                    sw.ElapsedMilliseconds);
            }
            else
            {
                logger.LogInformation(
                    "Step {StepAlias} completed successfully. Duration={Duration}ms",
                    context.StepExecution.StepAlias,
                    sw.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex,
                "Step {StepAlias} failed after {Duration}ms. Error={Error}",
                context.StepExecution.StepAlias,
                sw.ElapsedMilliseconds,
                ex.Message);
            throw;
        }
    }
}
