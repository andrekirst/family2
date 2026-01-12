using System.Collections.Concurrent;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// EF Core interceptor that counts SELECT queries executed against the database.
/// Thread-safe for parallel test execution using AsyncLocal to track test context.
/// </summary>
public sealed class QueryCountingInterceptor : DbCommandInterceptor
{
    // Thread-safe counter using ConcurrentDictionary keyed by test ID
    private static readonly ConcurrentDictionary<string, int> QueryCounts = new();

    // Thread-safe storage for executed queries (for debugging)
    private static readonly ConcurrentDictionary<string, ConcurrentBag<string>> ExecutedQueries = new();

    // AsyncLocal to track which test is currently executing
    private static readonly AsyncLocal<string?> CurrentTestId = new();

    /// <summary>
    /// Sets the current test ID for tracking queries in parallel tests.
    /// Call this at the start of each test.
    /// </summary>
    public static void SetCurrentTestId(string testId)
    {
        CurrentTestId.Value = testId;
        QueryCounts.TryAdd(testId, 0);
        ExecutedQueries.TryAdd(testId, []);
    }

    /// <summary>
    /// Gets the query count for a specific test.
    /// </summary>
    public static int GetQueryCount(string testId)
    {
        return QueryCounts.TryGetValue(testId, out var count) ? count : 0;
    }

    /// <summary>
    /// Gets the executed queries for a specific test (for debugging).
    /// </summary>
    public static IReadOnlyList<string> GetExecutedQueries(string testId)
    {
        return ExecutedQueries.TryGetValue(testId, out var queries)
            ? [.. queries]
            : [];
    }

    /// <summary>
    /// Resets the query count for a specific test.
    /// Call this before each test scenario to get accurate counts.
    /// </summary>
    public static void ResetQueryCount(string testId)
    {
        QueryCounts[testId] = 0;
        if (ExecutedQueries.TryGetValue(testId, out var queries))
        {
            queries.Clear();
        }
    }

    /// <summary>
    /// Clears all query counts (for test cleanup).
    /// </summary>
    public static void ClearAll()
    {
        QueryCounts.Clear();
        ExecutedQueries.Clear();
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        IncrementIfSelect(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        IncrementIfSelect(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    private static void IncrementIfSelect(DbCommand command)
    {
        var testId = CurrentTestId.Value;
        if (testId == null) return;

        var commandText = command.CommandText;
        // Only count SELECT queries (not INSERT, UPDATE, DELETE, or schema operations)
        if (commandText.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            QueryCounts.AddOrUpdate(testId, 1, (_, count) => count + 1);

            if (ExecutedQueries.TryGetValue(testId, out var queries))
            {
                queries.Add(commandText);
            }
        }
    }
}
