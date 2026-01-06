using FamilyHub.Modules.Auth.Domain.Events;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain;

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
    public UserRole Role { get; private set; }

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
        Role = UserRole.Member;
        Status = InvitationStatus.Pending;
    }

    private FamilyMemberInvitation(InvitationId id) : base(id)
    {
        FamilyId = FamilyId.From(Guid.Empty);
        Token = InvitationToken.Generate();
        DisplayCode = InvitationDisplayCode.Generate();
        Role = UserRole.Member;
        Status = InvitationStatus.Pending;
    }

    /// <summary>
    /// Creates a new email-based invitation.
    /// </summary>
    public static FamilyMemberInvitation CreateEmailInvitation(
        FamilyId familyId,
        Email email,
        UserRole role,
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
            isResend: false
        ));

        return invitation;
    }


    /// <summary>
    /// Accepts the invitation.
    /// </summary>
    /// <exception cref="InvalidOperationException">If invitation is not in pending status or has expired.</exception>
    public void Accept(UserId userId)
    {
        if (Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot accept invitation in {Status.Value} status. Only pending invitations can be accepted.");
        }

        if (DateTime.UtcNow > ExpiresAt)
        {
            Status = InvitationStatus.Expired;
            throw new InvalidOperationException("Invitation has expired and cannot be accepted.");
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
    public void UpdateRole(UserRole newRole)
    {
        if (Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Can only update role of pending invitations.");
        }

        if (newRole == UserRole.Owner)
        {
            throw new InvalidOperationException("Cannot update role to OWNER.");
        }

        Role = newRole;
    }
}
