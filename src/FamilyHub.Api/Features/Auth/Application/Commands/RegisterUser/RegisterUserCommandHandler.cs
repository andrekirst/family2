using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;

namespace FamilyHub.Api.Features.Auth.Application.Commands.RegisterUser;

/// <summary>
/// Handler for RegisterUserCommand.
/// Registers a new user or updates existing user from OAuth provider.
/// </summary>
public sealed class RegisterUserCommandHandler(IUserRepository userRepository)
    : ICommandHandler<RegisterUserCommand, RegisterUserResult>
{
    public async ValueTask<RegisterUserResult> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        // Check if user already exists by external ID
        var existingUser = await userRepository.GetByExternalIdAsync(command.ExternalUserId, cancellationToken);

        if (existingUser is not null)
        {
            // Update existing user's profile and login timestamp
            existingUser.UpdateProfile(command.Email, command.Name, command.EmailVerified);
            existingUser.UpdateLastLogin(DateTime.UtcNow);

            return new RegisterUserResult(existingUser.Id, IsNewUser: false);
        }

        // Check if user exists by email (e.g. Keycloak realm was recreated, external ID changed)
        var emailExists = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (emailExists is not null)
        {
            // Re-link to the new external ID and update profile
            emailExists.UpdateExternalId(command.ExternalUserId);
            emailExists.UpdateProfile(command.Email, command.Name, command.EmailVerified);
            emailExists.UpdateLastLogin(DateTime.UtcNow);

            return new RegisterUserResult(emailExists.Id, IsNewUser: false);
        }

        // Register new user
        var newUser = User.Register(
            command.Email,
            command.Name,
            command.ExternalUserId,
            command.EmailVerified,
            command.Username
        );

        await userRepository.AddAsync(newUser, cancellationToken);

        return new RegisterUserResult(newUser.Id, IsNewUser: true);
    }
}
