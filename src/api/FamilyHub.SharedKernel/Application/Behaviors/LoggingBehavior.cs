using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FamilyHub.SharedKernel.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior for logging command/query execution.
/// Automatically logs start, completion, and duration of all MediatR requests.
/// Uses LoggerMessage.Define for high-performance logging (CA1873 compliant).
/// </summary>
public sealed partial class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            LogRequestStarting(requestName);

            var response = await next(cancellationToken);

            stopwatch.Stop();
            LogRequestCompleted(requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogRequestFailed(requestName, stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
    }

    [LoggerMessage(LogLevel.Information, "Executing {requestName}")]
    partial void LogRequestStarting(string requestName);

    [LoggerMessage(LogLevel.Information, "Completed {requestName} in {elapsedMilliseconds}ms")]
    partial void LogRequestCompleted(string requestName, long elapsedMilliseconds);

    [LoggerMessage(LogLevel.Error, "Failed {requestName} after {elapsedMilliseconds}ms")]
    partial void LogRequestFailed(string requestName, long elapsedMilliseconds, Exception exception);
}
