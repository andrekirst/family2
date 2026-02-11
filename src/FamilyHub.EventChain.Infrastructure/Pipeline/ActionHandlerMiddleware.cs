using FamilyHub.EventChain.Infrastructure.Registry;

namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public sealed class ActionHandlerMiddleware(
    IChainRegistry registry) : IStepMiddleware
{
    public async Task InvokeAsync(StepPipelineContext context, StepDelegate next, CancellationToken ct)
    {
        if (context.ShouldSkip)
        {
            context.StepExecution.MarkSkipped();
            return;
        }

        var handler = registry.GetActionHandler(
            context.StepDefinition.ActionType,
            context.StepDefinition.ActionVersion.Value);

        if (handler is null)
        {
            throw new InvalidOperationException(
                $"No handler found for action {context.StepDefinition.ActionType}@{context.StepDefinition.ActionVersion.Value}");
        }

        context.StepExecution.MarkRunning();

        var actionContext = new ActionExecutionContext(
            context.StepExecution.InputPayload ?? "{}",
            context.ExecutionContext,
            context.CorrelationId);

        var result = await handler.ExecuteAsync(actionContext, ct);
        context.Result = result;

        if (result.Success)
        {
            context.StepExecution.MarkCompleted(result.OutputPayload);

            if (result.OutputPayload is not null)
            {
                context.ExecutionContext.SetStepOutput(
                    context.StepExecution.StepAlias,
                    result.OutputPayload);
            }
        }
        else
        {
            throw new InvalidOperationException(
                result.ErrorMessage ?? $"Action {context.StepDefinition.ActionType} failed");
        }

        // Continue pipeline (no-op at this point since this is the innermost middleware)
        await next(context, ct);
    }
}
