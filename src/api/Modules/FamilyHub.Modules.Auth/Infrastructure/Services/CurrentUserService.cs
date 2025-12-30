using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace FamilyHub.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Implementation of current user service using HTTP context to access JWT claims.
/// Works with Zitadel OAuth - maps Zitadel's 'sub' claim to internal UserId.
/// </summary>
public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository)
    : ICurrentUserService
{
    /// <inheritdoc />
    public UserId? GetUserId()
    {
        // Get Zitadel's 'sub' claim (their external user ID)
        var zitadelUserId = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContextAccessor.HttpContext?.User
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(zitadelUserId))
        {
            return null;
        }

        // Look up internal UserId by Zitadel's external user ID
        var user = userRepository
            .GetByExternalUserIdAsync(zitadelUserId, "zitadel", CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        return user?.Id;
    }

    /// <inheritdoc />
    public Email? GetUserEmail()
    {
        // Try standard claim types first (ASP.NET Core maps 'email' to Email)
        var emailClaim = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value
            ?? httpContextAccessor.HttpContext?.User
            .FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        if (string.IsNullOrEmpty(emailClaim))
        {
            return null;
        }

        return Email.From(emailClaim);
    }

    /// <inheritdoc />
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
