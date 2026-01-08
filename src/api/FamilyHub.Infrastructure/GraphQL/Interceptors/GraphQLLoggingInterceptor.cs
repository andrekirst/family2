using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using System.Diagnostics;

namespace FamilyHub.Infrastructure.GraphQL.Interceptors;

/// <summary>
/// GraphQL logging interceptor for Hot Chocolate.
/// Logs GraphQL operations (queries/mutations) and field resolutions.
/// Uses LoggerMessage.Define for high-performance logging (CA1873 compliant).
/// </summary>
public sealed class GraphQlLoggingInterceptor(ILogger<GraphQlLoggingInterceptor> logger) : ExecutionDiagnosticEventListener
{
    // High-performance logging delegates
    private static readonly Action<ILogger, string, string?, Exception?> LogOperationStarting =
        LoggerMessage.Define<string, string?>(
            LogLevel.Information,
            new EventId(2000, nameof(LogOperationStarting)),
            "GraphQL: Executing {OperationType} operation '{OperationName}'");

    private static readonly Action<ILogger, string, string?, long, Exception?> LogOperationCompleted =
        LoggerMessage.Define<string, string?, long>(
            LogLevel.Information,
            new EventId(2001, nameof(LogOperationCompleted)),
            "GraphQL: Completed {OperationType} operation '{OperationName}' in {ElapsedMilliseconds}ms");

    private static readonly Action<ILogger, string, string?, long, Exception?> LogOperationFailed =
        LoggerMessage.Define<string, string?, long>(
            LogLevel.Error,
            new EventId(2002, nameof(LogOperationFailed)),
            "GraphQL: Failed {OperationType} operation '{OperationName}' after {ElapsedMilliseconds}ms");

    public override IDisposable ExecuteRequest(IRequestContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var operationType = context.Operation?.Type.ToString() ?? "Unknown";
        var operationName = context.Request.OperationName ?? context.Operation?.Name ?? "Anonymous";

        LogOperationStarting(logger, operationType, operationName, null);

        return new RequestScope(stopwatch, operationType, operationName, logger);
    }

    private sealed class RequestScope(
        Stopwatch stopwatch,
        string operationType,
        string? operationName,
        ILogger<GraphQlLoggingInterceptor> logger)
        : IDisposable
    {
        public void Dispose()
        {
            stopwatch.Stop();
            LogOperationCompleted(logger, operationType, operationName, stopwatch.ElapsedMilliseconds, null);
        }
    }

    public override void RequestError(IRequestContext context, Exception exception)
    {
        var operationType = context.Operation?.Type.ToString() ?? "Unknown";
        var operationName = context.Request.OperationName ?? context.Operation?.Name ?? "Anonymous";

        LogOperationFailed(logger, operationType, operationName, 0, exception);
    }
}
