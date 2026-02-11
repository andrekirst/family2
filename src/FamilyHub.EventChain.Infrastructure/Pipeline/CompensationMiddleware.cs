using FamilyHub.EventChain.Infrastructure.Registry;
using Microsoft.Extensions.Logging;

namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public sealed partial class CompensationMiddleware(
    IChainRegistry registry,
    ILogger<CompensationMiddleware> logger) : IStepMiddleware
{
    public async Task InvokeAsync(StepPipelineContext context, StepDelegate next, CancellationToken ct)
    {
        try
        {
            await next(context, ct);
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            context.StepExecution.MarkFailed(ex.Message);

            // If this step had completed actions that are compensatable, trigger compensation
            if (context.StepDefinition is not { IsCompensatable: true, CompensationActionType: not null })
            {
                throw;
            }

            try
            {
                LogCompensatingStepStepaliasWithActionCompensationaction(logger, context.StepExecution.StepAlias, context.StepDefinition.CompensationActionType);

                context.StepExecution.MarkCompensating();

                var handler = registry.GetActionHandler(
                    context.StepDefinition.CompensationActionType,
                    context.StepDefinition.ActionVersion.Value);

                if (handler is not null)
                {
                    var compensationContext = new ActionExecutionContext(
                        context.StepExecution.OutputPayload ?? "{}",
                        context.ExecutionContext,
                        context.CorrelationId);

                    await handler.CompensateAsync(compensationContext, ct);
                    context.StepExecution.MarkCompensated();
                }
            }
            catch (Exception)
            {
                LogCompensationFailedForStepStepalias(logger, context.StepExecution.StepAlias);
            }

            throw;
        }
    }

    [LoggerMessage(LogLevel.Information, "Compensating step {stepAlias} with action {compensationAction}")]
    static partial void LogCompensatingStepStepaliasWithActionCompensationaction(ILogger<CompensationMiddleware> logger, string stepAlias, string compensationAction);

    [LoggerMessage(LogLevel.Error, "Compensation failed for step {stepAlias}")]
    static partial void LogCompensationFailedForStepStepalias(ILogger<CompensationMiddleware> logger, string stepAlias);
}
