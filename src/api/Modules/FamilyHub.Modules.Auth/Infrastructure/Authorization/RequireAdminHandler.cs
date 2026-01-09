using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Infrastructure.Authorization;

/// <summary>
/// Authorization handler that checks if the user has Admin role.
/// Checks role from ClaimsPrincipal (created by AuthorizationBehavior from IUserContext).
/// </summary>
public sealed partial class RequireAdminHandler(
    ILogger<RequireAdminHandler> logger)
    : AuthorizationHandler<RequireAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequireAdminRequirement requirement)
    {
        // Get role claim from ClaimsPrincipal (created by AuthorizationBehavior)
        var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(roleClaim))
        {
            LogAuthorizationFailedNoRoleClaimFound(logger);
            context.Fail();
            return Task.CompletedTask;
        }

        // Check if user has Admin role (case-insensitive)
        if (roleClaim.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            LogAuthorizationSucceededUserHasAdminRole(logger);
            context.Succeed(requirement);
        }
        else
        {
            LogAuthorizationFailedUserHasRoleRequiresAdmin(logger, roleClaim);
            context.Fail();
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Warning, "Authorization failed: No role claim found")]
    static partial void LogAuthorizationFailedNoRoleClaimFound(ILogger<RequireAdminHandler> logger);

    [LoggerMessage(LogLevel.Debug, "Authorization succeeded: User has 'admin' role")]
    static partial void LogAuthorizationSucceededUserHasAdminRole(ILogger<RequireAdminHandler> logger);

    [LoggerMessage(LogLevel.Warning, "Authorization failed: User has role '{role}' (requires 'admin')")]
    static partial void LogAuthorizationFailedUserHasRoleRequiresAdmin(ILogger<RequireAdminHandler> logger, string role);
}
