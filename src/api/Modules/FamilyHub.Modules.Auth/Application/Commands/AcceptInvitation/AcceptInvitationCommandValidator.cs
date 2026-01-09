using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Services;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FluentValidation;

namespace FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;

/// <summary>
/// Validator for AcceptInvitationCommand.
/// Validates invitation token, email match, status, expiration, and family existence.
/// Requires IUserContext (populated by UserContextEnrichmentBehavior) and repositories.
/// Uses CustomAsync pattern to cache validated entities for handler consumption.
/// </summary>
public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator(
        IFamilyMemberInvitationRepository invitationRepository,
        IFamilyRepository familyRepository,
        IUserContext userContext,
        TimeProvider timeProvider,
        IValidationCache validationCache)
    {
        RuleFor(x => x.Token)
            .CustomAsync(async (token, context, cancellationToken) =>
            {
                // 1. Token existence check
                var invitation = await invitationRepository.GetByTokenAsync(token, cancellationToken);
                if (invitation == null)
                {
                    context.AddFailure("Invalid or expired invitation token.");
                    return;
                }

                // 2. Status check
                if (invitation.Status != InvitationStatus.Pending)
                {
                    context.AddFailure($"Cannot accept invitation in {invitation.Status.Value} status. Only pending invitations can be accepted.");
                    return;
                }

                // 3. Expiration check
                if (timeProvider.GetUtcNow() > invitation.ExpiresAt)
                {
                    context.AddFailure("Invitation has expired and cannot be accepted.");
                    return;
                }

                // 4. Email match check
                if (invitation.Email != userContext.User.Email)
                {
                    context.AddFailure("Invitation email does not match authenticated user.");
                    return;
                }

                // 5. Family exists check
                var family = await familyRepository.GetByIdAsync(invitation.FamilyId, cancellationToken);
                if (family == null)
                {
                    context.AddFailure("Family not found.");
                    return;
                }

                // Cache entities for handler (eliminates duplicate database queries)
                validationCache.Set(CacheKeyBuilder.FamilyMemberInvitation(token.Value), invitation);
                validationCache.Set(CacheKeyBuilder.Family(invitation.FamilyId.Value), family);
            });
    }
}
