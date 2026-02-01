using FamilyHub.Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Common.Middleware;

/// <summary>
/// Middleware to set PostgreSQL Row-Level Security (RLS) session variables from JWT claims
/// This enforces multi-tenant data isolation at the database level
/// </summary>
public class PostgresRlsMiddleware
{
    private readonly RequestDelegate _next;

    public PostgresRlsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        // Only set RLS variables if user is authenticated
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Extract user ID from JWT 'sub' claim (Keycloak user ID)
            var externalUserId = context.User.FindFirst("sub")?.Value;

            // Extract family ID from custom JWT claim (if present)
            var familyId = context.User.FindFirst("family_id")?.Value;

            // Get internal user ID from database
            if (!string.IsNullOrEmpty(externalUserId))
            {
                var user = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.ExternalUserId == externalUserId);

                if (user != null)
                {
                    // Set app.current_user_id for RLS policies
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "SELECT set_config('app.current_user_id', {0}, false)",
                        user.Id.ToString());

                    // Set app.current_family_id if user has a family
                    if (user.FamilyId.HasValue)
                    {
                        await dbContext.Database.ExecuteSqlRawAsync(
                            "SELECT set_config('app.current_family_id', {0}, false)",
                            user.FamilyId.Value.ToString());
                    }
                    else if (!string.IsNullOrEmpty(familyId))
                    {
                        // Use family_id from JWT if available (backup)
                        await dbContext.Database.ExecuteSqlRawAsync(
                            "SELECT set_config('app.current_family_id', {0}, false)",
                            familyId);
                    }
                }
            }
        }

        await _next(context);
    }
}
