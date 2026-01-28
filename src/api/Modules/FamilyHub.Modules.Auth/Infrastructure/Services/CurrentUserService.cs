using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace FamilyHub.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Implementation of current user service using HTTP context to access JWT claims.
/// Works with local JWT authentication - the 'sub' claim contains the internal UserId.
/// </summary>
/// <param name="httpContextAccessor">Accessor for the HTTP context containing JWT claims.</param>
/// <param name="userRepository">Repository for user data access.</param>
public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository)
    : ICurrentUserService
{
    /// <summary>
    /// Custom claim type for family ID in JWT tokens.
    /// </summary>
    public const string FamilyIdClaimType = "family_id";

    /// <inheritdoc />
    public UserId GetUserId()
    {
        var userId = TryGetUserId();
        return userId ?? throw new UnauthorizedAccessException("User is not authenticated. No user ID claim found in JWT token.");
    }

    /// <inheritdoc />
    public async Task<UserId> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        // Verify user exists in database
        var exists = await userRepository.ExistsAsync(userId, cancellationToken);
        return !exists ? throw new UnauthorizedAccessException($"User with ID '{userId.Value}' not found in database.") : userId;
    }

    /// <inheritdoc />
    public Email? GetUserEmail()
    {
        // Try standard claim types (ASP.NET Core maps 'email' to Email)
        var emailClaim = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value
            ?? httpContextAccessor.HttpContext?.User
            .FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        return string.IsNullOrEmpty(emailClaim) ? null : Email.From(emailClaim);
    }

    /// <inheritdoc />
    public FamilyId? GetFamilyId() => TryGetFamilyId();

    /// <inheritdoc />
    public UserId? TryGetUserId()
    {
        // Get the 'sub' claim which contains the internal UserId
        var userIdClaim = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContextAccessor.HttpContext?.User
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return null;
        }

        // Parse the claim as a GUID (internal UserId)
        if (!Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return null;
        }

        return UserId.From(userIdGuid);
    }

    /// <inheritdoc />
    public FamilyId? TryGetFamilyId()
    {
        // Get the 'family_id' custom claim
        var familyIdClaim = httpContextAccessor.HttpContext?.User
            .FindFirst(FamilyIdClaimType)?.Value;

        if (string.IsNullOrEmpty(familyIdClaim))
        {
            return null;
        }

        // Parse the claim as a GUID
        if (!Guid.TryParse(familyIdClaim, out var familyIdGuid))
        {
            return null;
        }

        return FamilyId.From(familyIdGuid);
    }

    /// <inheritdoc />
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
