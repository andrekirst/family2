using FamilyHub.Modules.Family.Domain.Events;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Aggregates;

/// <summary>
/// Aggregate root representing a family member invitation.
/// Email-based invitations only.
/// </summary>
public class FamilyMemberInvitation : AggregateRoot<InvitationId>
{
    /// <summary>
    /// Family the invitation belongs to.
    /// </summary>
    public FamilyId FamilyId { get; private set; }

    /// <summary>
    /// Email address of the invited person.
    /// Required for all invitations.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Role to be assigned when the invitation is accepted.
    /// </summary>
    public FamilyRole Role { get; private set; }

    /// <summary>
    /// Secure token for accepting the invitation.
    /// </summary>
    public InvitationToken Token { get; private set; }

    /// <summary>
    /// User-friendly display code for debugging and support.
    /// </summary>
    public InvitationDisplayCode DisplayCode { get; private set; }

    /// <summary>
    /// When the invitation expires.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// User who created the invitation.
    /// </summary>
    public UserId InvitedByUserId { get; private set; }

    /// <summary>
    /// Current status of the invitation.
    /// </summary>
    public InvitationStatus Status { get; private set; }

    /// <summary>
    /// Optional personal message for the invitation.
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// When the invitation was accepted (null if not accepted).
    /// </summary>
    public DateTime? AcceptedAt { get; private set; }

    // Private constructor for EF Core
    private FamilyMemberInvitation() : base(InvitationId.From(Guid.Empty))
    {
        FamilyId = FamilyId.From(Guid.Empty);
        Email = Email.From("placeholder@example.com"); // EF Core will set actual value
        Token = InvitationToken.Generate(); // Placeholder
        DisplayCode = InvitationDisplayCode.Generate(); // Placeholder
        Role = FamilyRole.Member;
        Status = InvitationStatus.Pending;
    }

    private FamilyMemberInvitation(InvitationId id) : base(id)
    {
        FamilyId = FamilyId.From(Guid.Empty);
        Token = InvitationToken.Generate();
        DisplayCode = InvitationDisplayCode.Generate();
        Role = FamilyRole.Member;
        Status = InvitationStatus.Pending;
    }

    /// <summary>
    /// Creates a new email-based invitation.
    /// </summary>
    public static FamilyMemberInvitation CreateEmailInvitation(
        FamilyId familyId,
        Email email,
        FamilyRole role,
        UserId invitedByUserId,
        string? message = null)
    {
        var invitation = new FamilyMemberInvitation(InvitationId.New())
        {
            FamilyId = familyId,
            Email = email,
            Role = role,
            Token = InvitationToken.Generate(),
            DisplayCode = InvitationDisplayCode.Generate(),
            ExpiresAt = DateTime.UtcNow.AddDays(14),
            InvitedByUserId = invitedByUserId,
            Status = InvitationStatus.Pending,
            Message = message
        };

        invitation.AddDomainEvent(new FamilyMemberInvitedEvent(
            eventVersion: 1,
            invitationId: invitation.Id,
            familyId: familyId,
            email: email,
            role: role,
            token: invitation.Token,
            expiresAt: invitation.ExpiresAt,
            invitedByUserId: invitedByUserId,
            message: message,
            isResend: false
        ));

        return invitation;
    }


    /// <summary>
    /// Accepts the invitation.
    /// IMPORTANT: Validation (status, expiration, email match) is handled by AcceptInvitationCommandValidator.
    /// This method should only be called after validation passes.
    /// </summary>
    /// <param name="userId">The user accepting the invitation.</param>
    /// <exception cref="InvalidOperationException">If called in invalid state (defensive check).</exception>
    public void Accept(UserId userId)
    {
        // Defensive check - validator should prevent this
        if (Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot accept invitation in {Status.Value} status. " +
                "This should have been prevented by validator.");
        }

        Status = InvitationStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;

        AddDomainEvent(new InvitationAcceptedEvent(
            eventVersion: 1,
            invitationId: Id,
            familyId: FamilyId,
            userId: userId,
            acceptedAt: AcceptedAt.Value
        ));
    }

    /// <summary>
    /// Cancels the invitation.
    /// </summary>
    /// <exception cref="InvalidOperationException">If invitation is not in pending status.</exception>
    public void Cancel(UserId canceledByUserId)
    {
        if (Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot cancel invitation in {Status.Value} status. Only pending invitations can be canceled.");
        }

        Status = InvitationStatus.Canceled;

        AddDomainEvent(new InvitationCanceledEvent(
            eventVersion: 1,
            invitationId: Id,
            familyId: FamilyId,
            canceledByUserId: canceledByUserId,
            canceledAt: DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Resends the invitation with a new token and extended expiration.
    /// </summary>
    /// <exception cref="InvalidOperationException">If invitation is not in pending or expired status.</exception>
    public void Resend(UserId resentByUserId)
    {
        if (Status != InvitationStatus.Expired && Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot resend invitation in {Status.Value} status. Only pending or expired invitations can be resent.");
        }

        // Generate new token and extend expiration
        Token = InvitationToken.Generate();
        ExpiresAt = DateTime.UtcNow.AddDays(14);
        Status = InvitationStatus.Pending;

        AddDomainEvent(new FamilyMemberInvitedEvent(
            eventVersion: 1,
            invitationId: Id,
            familyId: FamilyId,
            email: Email,
            role: Role,
            token: Token,
            expiresAt: ExpiresAt,
            invitedByUserId: resentByUserId,
            message: Message, // Use existing message when resending
            isResend: true
        ));
    }

    /// <summary>
    /// Marks the invitation as accepted for managed accounts (after successful account creation).
    /// Called from command handler after Zitadel account creation.
    /// </summary>
    public void MarkAsAccepted(UserId userId)
    {
        Status = InvitationStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;

        AddDomainEvent(new InvitationAcceptedEvent(
            eventVersion: 1,
            invitationId: Id,
            familyId: FamilyId,
            userId: userId,
            acceptedAt: AcceptedAt.Value
        ));
    }

    /// <summary>
    /// Updates the role of a pending invitation.
    /// </summary>
    /// <exception cref="InvalidOperationException">If invitation is not pending or role is OWNER.</exception>
    public void UpdateRole(FamilyRole newRole)
    {
        if (Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Can only update role of pending invitations.");
        }

        if (newRole == FamilyRole.Owner)
        {
            throw new InvalidOperationException("Cannot update role to OWNER.");
        }

        Role = newRole;
    }
}
