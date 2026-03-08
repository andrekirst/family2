using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;

/// <summary>
/// Business validator for SendInvitationCommand.
/// Checks that a pending invitation does not already exist for the same email and family.
/// </summary>
public sealed class SendInvitationBusinessValidator : AbstractValidator<SendInvitationCommand>, IBusinessValidator<SendInvitationCommand>
{
    public SendInvitationBusinessValidator(
        IFamilyInvitationRepository invitationRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var existing = await invitationRepository.GetByEmailAndFamilyAsync(command.InviteeEmail, command.FamilyId, ct);
                return existing is null;
            })
            .WithErrorCode(DomainErrorCodes.DuplicateInvitation)
            .WithMessage(_ => localizer[DomainErrorCodes.DuplicateInvitation].Value);
    }
}
