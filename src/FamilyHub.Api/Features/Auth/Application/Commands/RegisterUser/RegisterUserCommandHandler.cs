using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;

namespace FamilyHub.Api.Features.Auth.Application.Commands.RegisterUser;

/// <summary>
/// Handler for RegisterUserCommand.
/// Registers a new user or updates existing user from OAuth provider.
/// Wolverine discovers this handler by convention (static Handle method).
/// </summary>
public static class RegisterUserCommandHandler
{
    public static async Task<RegisterUserResult> Handle(
        RegisterUserCommand command,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        // Check if user already exists by external ID
        var existingUser = await userRepository.GetByExternalIdAsync(command.ExternalUserId, ct);

        if (existingUser is not null)
        {
            // Update existing user's profile and login timestamp
            existingUser.UpdateProfile(command.Email, command.Name, command.EmailVerified);
            existingUser.UpdateLastLogin(DateTime.UtcNow);

            await userRepository.SaveChangesAsync(ct);

            return new RegisterUserResult(existingUser.Id, IsNewUser: false);
        }

        // Check for duplicate email
        var emailExists = await userRepository.GetByEmailAsync(command.Email, ct);
        if (emailExists is not null)
        {
            throw new InvalidOperationException($"User with email {command.Email.Value} already exists");
        }

        // Register new user
        var newUser = User.Register(
            command.Email,
            command.Name,
            command.ExternalUserId,
            command.EmailVerified,
            command.Username
        );

        await userRepository.AddAsync(newUser, ct);
        await userRepository.SaveChangesAsync(ct);

        return new RegisterUserResult(newUser.Id, IsNewUser: true);
    }
}
