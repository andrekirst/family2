using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Auth.Services;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Auth.GraphQL;

/// <summary>
/// GraphQL queries for authentication and user data
/// </summary>
public class AuthQueries
{
    /// <summary>
    /// Get the currently authenticated user's profile
    /// </summary>
    [Authorize]
    public async Task<UserDto?> GetCurrentUser(
        ClaimsPrincipal claimsPrincipal,
        [Service] AuthService authService)
    {
        var externalUserId = claimsPrincipal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(externalUserId))
        {
            return null;
        }

        return await authService.GetUserByExternalIdAsync(externalUserId);
    }

    /// <summary>
    /// Get user by ID (admin only - for now just authenticated)
    /// </summary>
    [Authorize]
    public async Task<UserDto?> GetUserById(
        Guid userId,
        [Service] AuthService authService)
    {
        return await authService.GetUserByIdAsync(userId);
    }
}
