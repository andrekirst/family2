using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FamilyHub.Modules.Auth.Infrastructure.Authorization;

/// <summary>
/// Authorization handler that checks if the user has Owner role.
/// Checks role from ClaimsPrincipal (created by AuthorizationBehavior from IUserContext).
/// </summary>
public sealed partial class RequireOwnerHandler(
    ILogger<RequireOwnerHandler> logger)
    : AuthorizationHandler<RequireOwnerRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequireOwnerRequirement requirement)
    {
        // Get role claim from ClaimsPrincipal (created by AuthorizationBehavior)
        var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(roleClaim))
        {
            LogAuthorizationFailedNoRoleClaimFound(logger);
            context.Fail();
            return Task.CompletedTask;
        }

        // Check if user has Owner role (case-insensitive)
        if (roleClaim.Equals("owner", StringComparison.OrdinalIgnoreCase))
        {
            LogAuthorizationSucceededUserHasOwnerRole(logger);
            context.Succeed(requirement);
        }
        else
        {
            LogAuthorizationFailedUserHasRoleRequiresOwner(logger, roleClaim);
            context.Fail();
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Warning, "Authorization failed: No role claim found")]
    static partial void LogAuthorizationFailedNoRoleClaimFound(ILogger<RequireOwnerHandler> logger);

    [LoggerMessage(LogLevel.Debug, "Authorization succeeded: User has 'owner' role")]
    static partial void LogAuthorizationSucceededUserHasOwnerRole(ILogger<RequireOwnerHandler> logger);

    [LoggerMessage(LogLevel.Warning, "Authorization failed: User has role '{role}' (requires 'owner')")]
    static partial void LogAuthorizationFailedUserHasRoleRequiresOwner(ILogger<RequireOwnerHandler> logger, string role);
}
