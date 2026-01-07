using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentValidation;

namespace FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;

/// <summary>
/// Validator for AcceptInvitationCommand.
/// Validates invitation token, email match, status, expiration, and family existence.
/// Requires IUserContext (populated by UserContextEnrichmentBehavior) and repositories.
/// </summary>
public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    private readonly IFamilyMemberInvitationRepository _invitationRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IUserContext _userContext;
    private readonly TimeProvider _timeProvider;
    private string? _lastValidationError;

    public AcceptInvitationCommandValidator(
        IFamilyMemberInvitationRepository invitationRepository,
        IFamilyRepository familyRepository,
        IUserContext userContext,
        TimeProvider timeProvider)
    {
        _invitationRepository = invitationRepository;
        _familyRepository = familyRepository;
        _userContext = userContext;
        _timeProvider = timeProvider;

        RuleFor(x => x.Token)
            .MustAsync(BeValidAcceptableInvitationWithExistingFamily)
            .WithMessage((_, token) => _lastValidationError ?? "Invalid invitation.");
    }

    /// <summary>
    /// Combined validation method that checks all requirements:
    /// 1. Invitation exists (token lookup)
    /// 2. Status is Pending
    /// 3. Not expired
    /// 4. Email matches authenticated user
    /// 5. Family exists
    /// </summary>
    private async Task<bool> BeValidAcceptableInvitationWithExistingFamily(
        InvitationToken token,
        CancellationToken cancellationToken)
    {
        // 1. Fetch invitation
        var invitation = await _invitationRepository.GetByTokenAsync(token, cancellationToken);
        if (invitation == null)
        {
            _lastValidationError = "Invalid or expired invitation token.";
            return false;
        }

        // 2. Check status
        if (invitation.Status != InvitationStatus.Pending)
        {
            _lastValidationError = $"Cannot accept invitation in {invitation.Status.Value} status. Only pending invitations can be accepted.";
            return false;
        }

        // 3. Check expiration
        if (_timeProvider.GetUtcNow() > invitation.ExpiresAt)
        {
            _lastValidationError = "Invitation has expired and cannot be accepted.";
            return false;
        }

        // 4. Check email match
        if (invitation.Email != _userContext.User.Email)
        {
            _lastValidationError = "Invitation email does not match authenticated user.";
            return false;
        }

        // 5. Check family exists
        var family = await _familyRepository.GetByIdAsync(invitation.FamilyId, cancellationToken);
        if (family == null)
        {
            _lastValidationError = "Family not found.";
            return false;
        }

        // All validations passed
        _lastValidationError = null;
        return true;
    }
}
