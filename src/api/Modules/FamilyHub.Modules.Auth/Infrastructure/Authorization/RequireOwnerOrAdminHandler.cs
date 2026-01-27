using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Infrastructure.Authorization;

/// <summary>
/// Authorization handler that checks if the user has Owner or Admin role.
/// Retrieves role from database using the UserId from JWT claims.
/// </summary>
public sealed partial class RequireOwnerOrAdminHandler(
    IUserRepository userRepository,
    ILogger<RequireOwnerOrAdminHandler> logger)
    : AuthorizationHandler<RequireOwnerOrAdminRequirement>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequireOwnerOrAdminRequirement requirement)
    {
        // Get the 'sub' claim which contains the internal UserId
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            LogAuthorizationFailedNoUserIdClaimFoundInJwtToken(logger);
            context.Fail();
            return;
        }

        // Parse the claim as a GUID (internal UserId)
        if (!Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            LogAuthorizationFailedInvalidUserIdFormat(logger, userIdClaim);
            context.Fail();
            return;
        }

        var userId = UserId.From(userIdGuid);

        // Look up user in database by internal ID
        var user = await userRepository.GetByIdAsync(userId, CancellationToken.None);

        if (user == null)
        {
            LogAuthorizationFailedUserNotFoundInDatabase(logger, userIdGuid);
            context.Fail();
            return;
        }

        // Check if user has Owner or Admin role
        var roleValue = user.Role.Value.ToLowerInvariant();

        if (roleValue is "owner" or "admin")
        {
            LogAuthorizationSucceeded(logger, userIdGuid, roleValue);
            context.Succeed(requirement);
        }
        else
        {
            LogAuthorizationFailedInsufficientRole(logger, userIdGuid, roleValue);
            context.Fail();
        }
    }

    [LoggerMessage(LogLevel.Warning, "Authorization failed: No user ID claim found in JWT token")]
    static partial void LogAuthorizationFailedNoUserIdClaimFoundInJwtToken(ILogger<RequireOwnerOrAdminHandler> logger);

    [LoggerMessage(LogLevel.Warning, "Authorization failed: Invalid user ID format in JWT token: '{userIdClaim}'")]
    static partial void LogAuthorizationFailedInvalidUserIdFormat(ILogger<RequireOwnerOrAdminHandler> logger, string userIdClaim);

    [LoggerMessage(LogLevel.Warning, "Authorization failed: User with ID '{userId}' not found in database")]
    static partial void LogAuthorizationFailedUserNotFoundInDatabase(ILogger<RequireOwnerOrAdminHandler> logger, Guid userId);

    [LoggerMessage(LogLevel.Debug, "Authorization succeeded: User {userId} has role '{role}'")]
    static partial void LogAuthorizationSucceeded(ILogger<RequireOwnerOrAdminHandler> logger, Guid userId, string role);

    [LoggerMessage(LogLevel.Warning, "Authorization failed: User {userId} has role '{role}' (requires 'owner' or 'admin')")]
    static partial void LogAuthorizationFailedInsufficientRole(ILogger<RequireOwnerOrAdminHandler> logger, Guid userId, string role);
}
