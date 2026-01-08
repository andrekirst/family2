using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Events;

/// <summary>
/// Domain event raised when a family member is invited via email.
/// Published when an email invitation is created or resent.
/// </summary>
public sealed class FamilyMemberInvitedEvent(
    int eventVersion,
    InvitationId invitationId,
    FamilyId familyId,
    Email email,
    FamilyRole role,
    InvitationToken token,
    DateTime expiresAt,
    UserId invitedByUserId,
    bool isResend = false)
    : DomainEvent
{
    /// <summary>
    /// Event schema version for future evolution.
    /// </summary>
    public int EventVersion { get; } = eventVersion;

    /// <summary>
    /// Unique invitation identifier.
    /// </summary>
    public InvitationId InvitationId { get; } = invitationId;

    /// <summary>
    /// Family the invitation belongs to.
    /// </summary>
    public FamilyId FamilyId { get; } = familyId;

    /// <summary>
    /// Email address of the invited person.
    /// </summary>
    public Email Email { get; } = email;

    /// <summary>
    /// Role to be assigned when the invitation is accepted.
    /// </summary>
    public FamilyRole Role { get; } = role;

    /// <summary>
    /// Secure token for accepting the invitation.
    /// </summary>
    public InvitationToken Token { get; } = token;

    /// <summary>
    /// When the invitation expires.
    /// </summary>
    public DateTime ExpiresAt { get; } = expiresAt;

    /// <summary>
    /// User who created the invitation.
    /// </summary>
    public UserId InvitedByUserId { get; } = invitedByUserId;

    /// <summary>
    /// Whether this is a resend of an existing invitation.
    /// </summary>
    public bool IsResend { get; } = isResend;
}
