using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Events;

/// <summary>
/// Domain event raised when an invitation is accepted.
/// Published when a user accepts an email-based invitation.
/// </summary>
public sealed class InvitationAcceptedEvent(
    int eventVersion,
    InvitationId invitationId,
    FamilyId familyId,
    UserId userId,
    DateTime acceptedAt)
    : DomainEvent
{
    /// <summary>
    /// Event schema version for future evolution.
    /// </summary>
    public int EventVersion { get; } = eventVersion;

    /// <summary>
    /// Invitation that was accepted.
    /// </summary>
    public InvitationId InvitationId { get; } = invitationId;

    /// <summary>
    /// Family the invitation belongs to.
    /// </summary>
    public FamilyId FamilyId { get; } = familyId;

    /// <summary>
    /// User who accepted the invitation.
    /// </summary>
    public UserId UserId { get; } = userId;

    /// <summary>
    /// When the invitation was accepted.
    /// </summary>
    public DateTime AcceptedAt { get; } = acceptedAt;
}
