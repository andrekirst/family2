using FamilyHub.Api.Features.Auth.Domain.Repositories;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateLastLogin;

/// <summary>
/// Handler for UpdateLastLoginCommand.
/// Updates a user's last login timestamp.
/// </summary>
public static class UpdateLastLoginCommandHandler
{
    public static async Task<bool> Handle(
        UpdateLastLoginCommand command,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetByExternalIdAsync(command.ExternalUserId, ct);
        if (user is null)
        {
            return false;
        }

        user.UpdateLastLogin(command.LoginTime);
        await userRepository.SaveChangesAsync(ct);

        return true;
    }
}
