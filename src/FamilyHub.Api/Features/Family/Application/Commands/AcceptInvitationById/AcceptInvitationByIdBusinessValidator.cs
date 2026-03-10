using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitationById;

/// <summary>
/// Business validator for AcceptInvitationByIdCommand.
/// Checks that the invitation exists, the user exists, and the user is not already a member.
/// </summary>
[SecurityCheck("IDOR")]
public sealed class AcceptInvitationByIdBusinessValidator : AbstractValidator<AcceptInvitationByIdCommand>, IBusinessValidator<AcceptInvitationByIdCommand>
{
    public AcceptInvitationByIdBusinessValidator(
        IFamilyInvitationRepository invitationRepository,
        IUserRepository userRepository,
        IFamilyMemberRepository memberRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
                await invitationRepository.ExistsByIdAsync(command.InvitationId!.Value, cancellationToken))
            .WithErrorCode(DomainErrorCodes.InvitationNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.InvitationNotFound].Value);

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
                await userRepository.ExistsByIdAsync(command.UserId, cancellationToken))
            .WithErrorCode(DomainErrorCodes.UserNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.UserNotFound].Value);

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
            {
                var invitation = await invitationRepository.GetByIdAsync(command.InvitationId!.Value, cancellationToken);
                if (invitation is null)
                {
                    return true;
                }

                return !await memberRepository.ExistsByUserAndFamilyAsync(command.UserId, invitation.FamilyId, cancellationToken);
            })
            .WithErrorCode(DomainErrorCodes.AlreadyFamilyMember)
            .WithMessage(_ => localizer[DomainErrorCodes.AlreadyFamilyMember].Value);
    }
}
