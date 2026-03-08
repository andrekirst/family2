using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.DeclineInvitationById;

/// <summary>
/// Business validator for DeclineInvitationByIdCommand.
/// Checks that the invitation and user exist.
/// </summary>
[SecurityCheck("IDOR")]
public sealed class DeclineInvitationByIdBusinessValidator : AbstractValidator<DeclineInvitationByIdCommand>, IBusinessValidator<DeclineInvitationByIdCommand>
{
    public DeclineInvitationByIdBusinessValidator(
        IFamilyInvitationRepository invitationRepository,
        IUserRepository userRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var invitation = await invitationRepository.GetByIdAsync(command.InvitationId, ct);
                return invitation is not null;
            })
            .WithErrorCode(DomainErrorCodes.InvitationNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.InvitationNotFound].Value);

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var user = await userRepository.GetByIdAsync(command.UserId, ct);
                return user is not null;
            })
            .WithErrorCode(DomainErrorCodes.UserNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.UserNotFound].Value);
    }
}
