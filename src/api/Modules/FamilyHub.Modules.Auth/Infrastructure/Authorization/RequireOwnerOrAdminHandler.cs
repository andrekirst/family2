using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FamilyHub.Modules.Auth.Infrastructure.Authorization;

/// <summary>
/// Authorization handler that checks if the user has Owner or Admin role.
/// Retrieves role from database, not from JWT claims.
/// </summary>
public sealed partial class RequireOwnerOrAdminHandler(
    IUserRepository userRepository,
    ILogger<RequireOwnerOrAdminHandler> logger)
    : AuthorizationHandler<RequireOwnerOrAdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequireOwnerOrAdminRequirement requirement)
    {
        // Get Zitadel's 'sub' claim (external user ID)
        var zitadelUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(zitadelUserId))
        {
            LogAuthorizationFailedNoUserIdClaimFoundInJwtToken(logger);
            context.Fail();
            return;
        }

        // Look up user in database by external ID
        var user = await userRepository.GetByExternalUserIdAsync(
            zitadelUserId,
            "zitadel",
            CancellationToken.None);

        if (user == null)
        {
            LogAuthorizationFailedUserWithExternalIdZitadeluseridNotFoundInDatabase(logger, zitadelUserId);
            context.Fail();
            return;
        }

        // Check if user has Owner or Admin role
        var roleValue = user.Role.Value.ToLowerInvariant();

        if (roleValue is "owner" or "admin")
        {
            LogAuthorizationSucceededUserUseridHasRoleRole(logger, user.Id.Value, roleValue);
            context.Succeed(requirement);
        }
        else
        {
            LogAuthorizationFailedUserUseridHasRoleRoleRequiresOwnerOrAdmin(logger, user.Id.Value, roleValue);
            context.Fail();
        }
    }

    [LoggerMessage(LogLevel.Warning, "Authorization failed: No user ID claim found in JWT token")]
    static partial void LogAuthorizationFailedNoUserIdClaimFoundInJwtToken(ILogger<RequireOwnerOrAdminHandler> logger);

    [LoggerMessage(LogLevel.Warning, "Authorization failed: User with external ID '{zitadelUserId}' not found in database")]
    static partial void LogAuthorizationFailedUserWithExternalIdZitadeluseridNotFoundInDatabase(ILogger<RequireOwnerOrAdminHandler> logger, string zitadelUserId);

    [LoggerMessage(LogLevel.Debug, "Authorization succeeded: User {userId} has role '{role}'")]
    static partial void LogAuthorizationSucceededUserUseridHasRoleRole(ILogger<RequireOwnerOrAdminHandler> logger, Guid userId, string role);

    [LoggerMessage(LogLevel.Warning, "Authorization failed: User {userId} has role '{role}' (requires 'owner' or 'admin')")]
    static partial void LogAuthorizationFailedUserUseridHasRoleRoleRequiresOwnerOrAdmin(ILogger<RequireOwnerOrAdminHandler> logger, Guid userId, string role);
}
