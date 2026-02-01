using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Auth.Services;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Auth.GraphQL;

/// <summary>
/// GraphQL mutations for authentication operations
/// </summary>
public class AuthMutations
{
    /// <summary>
    /// Register or update a user from OAuth callback
    /// This is called automatically when a user completes OAuth login
    /// </summary>
    [Authorize]
    public async Task<UserDto> RegisterUser(
        RegisterUserRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] AuthService authService)
    {
        // Extract OAuth claims from JWT
        var externalUserId = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("Invalid token: missing sub claim");

        var email = claimsPrincipal.FindFirst("email")?.Value
            ?? throw new UnauthorizedAccessException("Invalid token: missing email claim");

        var name = claimsPrincipal.FindFirst("name")?.Value ?? email;
        var emailVerified = bool.Parse(claimsPrincipal.FindFirst("email_verified")?.Value ?? "false");

        // Create request with OAuth data
        var request = new RegisterUserRequest
        {
            Email = email,
            Name = name,
            ExternalUserId = externalUserId,
            ExternalProvider = "KEYCLOAK",
            EmailVerified = emailVerified
        };

        return await authService.RegisterUserAsync(request);
    }
}
