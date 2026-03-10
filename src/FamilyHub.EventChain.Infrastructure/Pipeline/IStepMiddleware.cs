namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public delegate Task StepDelegate(StepPipelineContext context, CancellationToken cancellationToken);

public interface IStepMiddleware
{
    Task InvokeAsync(StepPipelineContext context, StepDelegate next, CancellationToken cancellationToken);
}
