using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.Repositories;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateUserLocale;

/// <summary>
/// Handler for UpdateUserLocaleCommand.
/// Looks up the user by external ID and updates their preferred locale.
/// </summary>
public sealed class UpdateUserLocaleCommandHandler(
    IUserRepository userRepository)
    : ICommandHandler<UpdateUserLocaleCommand, UpdateUserLocaleResult>
{
    public async ValueTask<UpdateUserLocaleResult> Handle(
        UpdateUserLocaleCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(command.ExternalUserId, cancellationToken)
            ?? throw new DomainException("User not found", DomainErrorCodes.UserNotFound);

        user.UpdateLocale(command.Locale);

        return new UpdateUserLocaleResult(true);
    }
}
