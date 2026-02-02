using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Application.Commands;
using FamilyHub.Api.Features.Auth.Application.Mappers;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Models;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Auth.GraphQL;

/// <summary>
/// GraphQL mutations for authentication operations.
/// Uses Input → Command pattern per ADR-003.
/// </summary>
public class AuthMutations
{
    /// <summary>
    /// Register or update a user from OAuth callback.
    /// This is called automatically when a user completes OAuth login.
    /// </summary>
    [Authorize]
    public async Task<UserDto> RegisterUser(
        RegisterUserRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        // Extract OAuth claims from JWT (don't trust client input!)
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("Invalid token: missing sub claim");

        var emailString = claimsPrincipal.FindFirst("email")?.Value
            ?? throw new UnauthorizedAccessException("Invalid token: missing email claim");

        var nameString = claimsPrincipal.FindFirst("name")?.Value ?? emailString;
        var emailVerified = bool.Parse(claimsPrincipal.FindFirst("email_verified")?.Value ?? "false");

        // Convert primitives to value objects (Input → Command pattern)
        var email = Email.From(emailString);
        var name = UserName.From(nameString);
        var externalUserId = ExternalUserId.From(externalUserIdString);

        // Create command
        var command = new RegisterUserCommand(email, name, externalUserId, emailVerified);

        // Send command via Wolverine
        var result = await commandBus.SendAsync<RegisterUserResult>(command, ct);

        // Query the registered user and map to DTO
        var registeredUser = await userRepository.GetByIdAsync(result.UserId, ct);
        if (registeredUser is null)
        {
            throw new InvalidOperationException("User registration failed");
        }

        return UserMapper.ToDto(registeredUser);
    }
}
