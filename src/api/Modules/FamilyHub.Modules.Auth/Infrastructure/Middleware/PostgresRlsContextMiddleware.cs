using System.Security.Claims;
using FamilyHub.Modules.Auth.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FamilyHub.Modules.Auth.Infrastructure.Middleware;

/// <summary>
/// Middleware that sets the PostgreSQL session variable 'app.current_user_id'
/// for Row-Level Security (RLS) policies.
///
/// EXECUTION ORDER:
/// 1. Authentication middleware populates User (ClaimsPrincipal)
/// 2. This middleware extracts user ID from JWT claims
/// 3. Sets PostgreSQL session variable for RLS policies
/// 4. GraphQL/MediatR request processing occurs
/// 5. PostgreSQL enforces RLS based on session variable
///
/// ARCHITECTURE DECISION:
/// - Uses 'sub' claim from JWT (Zitadel's standard user identifier claim)
/// - Session variable is transaction-scoped (true parameter in set_config)
/// - Automatically cleared after each request (no cleanup needed)
/// - Runs on every request, even unauthenticated (RLS handles NULL user_id gracefully)
///
/// SECURITY:
/// - RLS policies will deny access if current_user_id() returns NULL (unauthenticated)
/// - SQL injection prevented by parameterized queries
/// - Transaction-scoped variables prevent cross-request leakage
///
/// PERFORMANCE:
/// - One additional SQL command per request (~1ms overhead)
/// - Eliminates need for family_id filters in application code
/// - RLS policies use indexed columns (family_id, user_id)
/// </summary>
public partial class PostgresRlsContextMiddleware(
    RequestDelegate next,
    ILogger<PostgresRlsContextMiddleware> logger)
{
    /// <summary>
    /// Processes the HTTP request, setting the PostgreSQL session variable for RLS.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="dbContext">The Auth database context for PostgreSQL connection access.</param>
    /// <returns>A task representing the asynchronous middleware operation.</returns>
    public async Task InvokeAsync(HttpContext context, AuthDbContext dbContext)
    {
        // Extract user ID from JWT claims (if authenticated)
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value; // Zitadel uses 'sub' claim

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            try
            {
                // Get the underlying PostgreSQL connection
                var connection = dbContext.Database.GetDbConnection();

                // Ensure connection is open
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                // Set the session variable for RLS policies
                // The 'true' parameter makes it transaction-scoped (cleared after transaction)
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT set_config('app.current_user_id', @userId, true)";

                var parameter = new NpgsqlParameter("@userId", System.Data.DbType.String)
                {
                    Value = userId.ToString()
                };
                cmd.Parameters.Add(parameter);

                await cmd.ExecuteNonQueryAsync();

                LogRlsContextSet(userId, context.Request.Path);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                // RLS will deny access if context not set (fail-secure)
                LogRlsContextSetFailed(userId, ex);
            }
        }
        else
        {
            // No user ID found - unauthenticated request
            // RLS policies will handle this by returning empty results for protected tables
            LogNoUserIdInClaims(context.Request.Path);
        }

        // Continue to next middleware (GraphQL, MediatR, etc.)
        await next(context);
    }

    [LoggerMessage(LogLevel.Debug, "PostgreSQL RLS context set for user {UserId} (Request: {RequestPath})")]
    partial void LogRlsContextSet(Guid userId, PathString requestPath);

    [LoggerMessage(LogLevel.Error, "Failed to set PostgreSQL RLS context for user {UserId}. Request will proceed but RLS policies will deny unauthorized access.")]
    partial void LogRlsContextSetFailed(Guid userId, Exception exception);

    [LoggerMessage(LogLevel.Debug, "No user ID found in claims (unauthenticated request: {RequestPath}). RLS policies will enforce access control.")]
    partial void LogNoUserIdInClaims(PathString requestPath);
}
