using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Application.Mappers;
using FamilyHub.Api.Features.Auth.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Auth.Application.Commands.RegisterUser;

[ExtendObjectType(typeof(RootMutation))]
public class MutationType
{
    /// <summary>
    /// Register or update a user from OAuth callback.
    /// This is called automatically when a user completes OAuth login.
    /// </summary>
    [Authorize]
    public async Task<UserDto> RegisterUser(
        RegisterUserRequest input,
        [Service] ICurrentUserContext currentUserContext,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        // Extract OAuth claims from JWT via ICurrentUserContext
        var claims = currentUserContext.GetRawClaims();

        var nameString = claims.UserName ?? claims.Email;

        var command = new RegisterUserCommand(
            Email.From(claims.Email),
            UserName.From(nameString),
            claims.ExternalUserId,
            claims.EmailVerified);
        var result = await commandBus.SendAsync(command, cancellationToken);

        return UserMapper.ToDto(result.RegisteredUser);
    }
}
