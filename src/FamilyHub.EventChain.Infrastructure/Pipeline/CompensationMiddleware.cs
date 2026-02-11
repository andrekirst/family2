using FamilyHub.EventChain.Infrastructure.Registry;
using Microsoft.Extensions.Logging;

namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public sealed class CompensationMiddleware(
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
            if (context.StepDefinition.IsCompensatable &&
                context.StepDefinition.CompensationActionType is not null)
            {
                try
                {
                    logger.LogInformation(
                        "Compensating step {StepAlias} with action {CompensationAction}",
                        context.StepExecution.StepAlias,
                        context.StepDefinition.CompensationActionType);

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
                catch (Exception compensationEx)
                {
                    logger.LogError(compensationEx,
                        "Compensation failed for step {StepAlias}",
                        context.StepExecution.StepAlias);
                }
            }

            throw;
        }
    }
}
