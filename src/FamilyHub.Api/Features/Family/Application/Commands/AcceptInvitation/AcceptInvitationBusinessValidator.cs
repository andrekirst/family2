using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitation;

/// <summary>
/// Business validator for AcceptInvitationCommand.
/// Checks that the invitation token is valid, the user exists, and the user is not already a member.
/// </summary>
public sealed class AcceptInvitationBusinessValidator : AbstractValidator<AcceptInvitationCommand>, IBusinessValidator<AcceptInvitationCommand>
{
    public AcceptInvitationBusinessValidator(
        IFamilyInvitationRepository invitationRepository,
        IUserRepository userRepository,
        IFamilyMemberRepository memberRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var tokenHash = SecureTokenHelper.ComputeSha256Hash(command.Token);
                var invitation = await invitationRepository.GetByTokenHashAsync(InvitationToken.From(tokenHash), ct);
                return invitation is not null;
            })
            .WithErrorCode(DomainErrorCodes.InvalidInvitationToken)
            .WithMessage(_ => localizer[DomainErrorCodes.InvalidInvitationToken].Value);

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var user = await userRepository.GetByIdAsync(command.UserId, ct);
                return user is not null;
            })
            .WithErrorCode(DomainErrorCodes.UserNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.UserNotFound].Value);

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var tokenHash = SecureTokenHelper.ComputeSha256Hash(command.Token);
                var invitation = await invitationRepository.GetByTokenHashAsync(InvitationToken.From(tokenHash), ct);
                if (invitation is null)
                {
                    return true;
                }

                var existingMember = await memberRepository.GetByUserAndFamilyAsync(command.UserId, invitation.FamilyId, ct);
                return existingMember is null;
            })
            .WithErrorCode(DomainErrorCodes.AlreadyFamilyMember)
            .WithMessage(_ => localizer[DomainErrorCodes.AlreadyFamilyMember].Value);
    }
}
