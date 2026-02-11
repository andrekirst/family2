using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Repositories;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateLastLogin;

/// <summary>
/// Handler for UpdateLastLoginCommand.
/// Updates a user's last login timestamp.
/// </summary>
public sealed class UpdateLastLoginCommandHandler(IUserRepository userRepository)
    : ICommandHandler<UpdateLastLoginCommand, bool>
{
    public async ValueTask<bool> Handle(
        UpdateLastLoginCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(command.ExternalUserId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.UpdateLastLogin(command.LoginTime);

        return true;
    }
}
