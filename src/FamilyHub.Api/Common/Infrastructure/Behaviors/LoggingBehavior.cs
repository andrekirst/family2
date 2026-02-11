using System.Diagnostics;
using FamilyHub.Api.Common.Modules;
using Mediator;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that logs command/query execution with timing.
/// </summary>
[PipelinePriority(PipelinePriorities.Logging)]
public sealed class LoggingBehavior<TMessage, TResponse>(
    ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var messageName = typeof(TMessage).Name;
        logger.LogInformation("Handling {MessageName}", messageName);

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next(message, cancellationToken);
            sw.Stop();
            logger.LogInformation("Handled {MessageName} in {ElapsedMs}ms", messageName, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Error handling {MessageName} after {ElapsedMs}ms", messageName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
