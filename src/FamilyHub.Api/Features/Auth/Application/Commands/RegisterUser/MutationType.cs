using System.Security.Claims;
using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Features.Auth.Application.Mappers;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.GraphQL;
using FamilyHub.Api.Features.Auth.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Auth.Application.Commands.RegisterUser;

[ExtendObjectType<AuthMutations>]
public class MutationType
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
        CancellationToken cancellationToken)
    {
        // Extract OAuth claims from JWT
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
                                   ?? throw new UnauthorizedAccessException("Invalid token: missing sub claim");

        var emailString = claimsPrincipal.FindFirst(ClaimNames.Email)?.Value
                          ?? throw new UnauthorizedAccessException("Invalid token: missing email claim");

        var nameString = claimsPrincipal.FindFirst(ClaimNames.Name)?.Value ?? emailString;
        var emailVerified = bool.Parse(claimsPrincipal.FindFirst(ClaimNames.EmailVerified)?.Value ?? "false");

        var command = new RegisterUserCommand(
            Email.From(emailString),
            UserName.From(nameString),
            ExternalUserId.From(externalUserIdString),
            emailVerified);
        var result = await commandBus.SendAsync(command, cancellationToken);

        // Query the registered user and map to DTO
        var registeredUser = await userRepository.GetByIdAsync(result.UserId, cancellationToken);
        if (registeredUser is null)
        {
            throw new InvalidOperationException("User registration failed");
        }

        return UserMapper.ToDto(registeredUser);
    }
}
