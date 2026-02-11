namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public sealed class StepPipeline(IEnumerable<IStepMiddleware> middlewares)
{
    private readonly IReadOnlyList<IStepMiddleware> _middlewares = middlewares.ToList().AsReadOnly();

    public Task ExecuteAsync(StepPipelineContext context, CancellationToken ct)
    {
        StepDelegate pipeline = (_, _) => Task.CompletedTask;

        // Build pipeline in reverse order so first middleware wraps everything
        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = pipeline;
            pipeline = (ctx, token) => middleware.InvokeAsync(ctx, next, token);
        }

        return pipeline(context, ct);
    }
}
