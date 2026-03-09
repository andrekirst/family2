using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FamilyHub.Api.Common.Infrastructure.Interceptors;

/// <summary>
/// EF Core interceptor that logs queries exceeding a configurable threshold.
/// Captures the command text, parameters, and execution time for performance monitoring.
/// </summary>
public sealed class SlowQueryInterceptor(
    ILogger<SlowQueryInterceptor> logger,
    TimeProvider timeProvider) : DbCommandInterceptor
{
    private const int SlowQueryThresholdMs = 100;

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        LogSlowQuery(command, eventData.Duration);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogSlowQuery(command, eventData.Duration);
        return ValueTask.FromResult(result);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        LogSlowQuery(command, eventData.Duration);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogSlowQuery(command, eventData.Duration);
        return ValueTask.FromResult(result);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        LogSlowQuery(command, eventData.Duration);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        LogSlowQuery(command, eventData.Duration);
        return ValueTask.FromResult(result);
    }

    private void LogSlowQuery(DbCommand command, TimeSpan duration)
    {
        if (duration.TotalMilliseconds < SlowQueryThresholdMs)
        {
            return;
        }

        logger.LogWarning(
            "Slow query detected ({DurationMs}ms): {CommandText}",
            (int)duration.TotalMilliseconds,
            command.CommandText);
    }
}
