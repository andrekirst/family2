using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Events;

/// <summary>
/// Domain event raised when an invitation is canceled.
/// Published when a family admin cancels a pending invitation.
/// </summary>
public sealed class InvitationCanceledEvent(
    int eventVersion,
    InvitationId invitationId,
    FamilyId familyId,
    UserId canceledByUserId,
    DateTime canceledAt)
    : DomainEvent
{
    /// <summary>
    /// Event schema version for future evolution.
    /// </summary>
    public int EventVersion { get; } = eventVersion;

    /// <summary>
    /// Invitation that was canceled.
    /// </summary>
    public InvitationId InvitationId { get; } = invitationId;

    /// <summary>
    /// Family the invitation belongs to.
    /// </summary>
    public FamilyId FamilyId { get; } = familyId;

    /// <summary>
    /// User who canceled the invitation.
    /// </summary>
    public UserId CanceledByUserId { get; } = canceledByUserId;

    /// <summary>
    /// When the invitation was canceled.
    /// </summary>
    public DateTime CanceledAt { get; } = canceledAt;
}
