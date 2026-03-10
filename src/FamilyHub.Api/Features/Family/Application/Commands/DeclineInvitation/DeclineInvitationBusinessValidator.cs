using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.DeclineInvitation;

/// <summary>
/// Business validator for DeclineInvitationCommand.
/// Checks that the invitation token is valid.
/// </summary>
public sealed class DeclineInvitationBusinessValidator : AbstractValidator<DeclineInvitationCommand>, IBusinessValidator<DeclineInvitationCommand>
{
    public DeclineInvitationBusinessValidator(
        IFamilyInvitationRepository invitationRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
            {
                var tokenHash = SecureTokenHelper.ComputeSha256Hash(command.Token);
                var invitation = await invitationRepository.GetByTokenHashAsync(InvitationToken.From(tokenHash), cancellationToken);
                return invitation is not null;
            })
            .WithErrorCode(DomainErrorCodes.InvalidInvitationToken)
            .WithMessage(_ => localizer[DomainErrorCodes.InvalidInvitationToken].Value);
    }
}
