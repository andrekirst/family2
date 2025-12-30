using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FamilyHub.SharedKernel.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior for logging command/query execution.
/// Automatically logs start, completion, and duration of all MediatR requests.
/// Uses LoggerMessage.Define for high-performance logging (CA1873 compliant).
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // High-performance logging delegates using LoggerMessage.Define
    private static readonly Action<ILogger, string, Exception?> LogRequestStarting =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1000, nameof(LogRequestStarting)),
            "Executing {RequestName}");

    private static readonly Action<ILogger, string, long, Exception?> LogRequestCompleted =
        LoggerMessage.Define<string, long>(
            LogLevel.Information,
            new EventId(1001, nameof(LogRequestCompleted)),
            "Completed {RequestName} in {ElapsedMilliseconds}ms");

    private static readonly Action<ILogger, string, long, Exception?> LogRequestFailed =
        LoggerMessage.Define<string, long>(
            LogLevel.Error,
            new EventId(1002, nameof(LogRequestFailed)),
            "Failed {RequestName} after {ElapsedMilliseconds}ms");

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            LogRequestStarting(_logger, requestName, null);

            var response = await next(cancellationToken);

            stopwatch.Stop();
            LogRequestCompleted(_logger, requestName, stopwatch.ElapsedMilliseconds, null);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogRequestFailed(_logger, requestName, stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
    }
}
