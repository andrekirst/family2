using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Common.Middleware;

/// <summary>
/// Middleware to set PostgreSQL Row-Level Security (RLS) session variables from database.
/// Uses a single PL/pgSQL DO block that runs as the connection owner (bypassing RLS)
/// to look up the user by JWT sub claim and set both session variables atomically.
/// This avoids a circular dependency where querying auth.users via EF Core would itself
/// be subject to the RLS policy that requires app.current_user_id to already be set.
///
/// Includes a single retry (500ms) to handle the race condition during first login,
/// where RegisterUser may not have committed the user record yet.
/// </summary>
public class PostgresRlsMiddleware(RequestDelegate next, ILogger<PostgresRlsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        // Only set RLS variables if user is authenticated
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Extract user ID from JWT 'sub' claim (Keycloak user ID)
            var externalUserId = context.User.FindFirst(ClaimNames.Standard.Sub)?.Value;

            // Validate as GUID to guarantee safe SQL embedding (Keycloak sub claims are UUIDs).
            // Canonical GUID format (hex + hyphens) cannot contain SQL injection characters.
            if (!string.IsNullOrEmpty(externalUserId) && Guid.TryParse(externalUserId, out var parsedId))
            {
                var safeExternalUserId = parsedId.ToString();

                await SetRlsSessionVariables(dbContext, safeExternalUserId);

                // Check if user was found; if not, retry once after 500ms.
                // Handles race condition during first login where RegisterUser hasn't committed yet.
                var currentUserId = await GetCurrentSessionUserId(dbContext);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    logger.LogDebug(
                        "RLS user lookup returned empty for {ExternalUserId}, retrying in 500ms",
                        safeExternalUserId);
                    await Task.Delay(500, context.RequestAborted);
                    await SetRlsSessionVariables(dbContext, safeExternalUserId);
                }
            }
        }

        await next(context);
    }

    // EF1002: Safe — value is GUID-validated by caller (hex + hyphens only).
#pragma warning disable EF1002
    private static async Task SetRlsSessionVariables(AppDbContext dbContext, string safeExternalUserId)
    {
        // Single PL/pgSQL block: lookup user by external ID, set both session variables.
        // DO blocks execute as the connection owner, bypassing RLS policies.
        await dbContext.Database.ExecuteSqlRawAsync(
            $"""
            DO $$
            DECLARE v_user_id text; v_family_id text;
            BEGIN
                SELECT "id"::text, "family_id"::text
                INTO v_user_id, v_family_id
                FROM auth.users
                WHERE "external_user_id" = '{safeExternalUserId}';
                IF v_user_id IS NOT NULL THEN
                    PERFORM set_config('app.current_user_id', v_user_id, false);
                    IF v_family_id IS NOT NULL THEN
                        PERFORM set_config('app.current_family_id', v_family_id, false);
                    END IF;
                END IF;
            END $$;
            """);
    }

    private static async Task<string?> GetCurrentSessionUserId(AppDbContext dbContext)
    {
        var result = await dbContext.Database
            .SqlQueryRaw<string>("SELECT current_setting('app.current_user_id', true) AS \"Value\"")
            .FirstOrDefaultAsync();
        return result;
    }
#pragma warning restore EF1002
}
