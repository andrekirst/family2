using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Events;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Entities;

/// <summary>
/// Family invitation aggregate root with full lifecycle management.
/// Tracks the invitation from creation through acceptance, decline, or revocation.
/// Token is stored as a SHA256 hash; plaintext is only in the email link.
/// </summary>
public sealed class FamilyInvitation : AggregateRoot<InvitationId>
{
#pragma warning disable CS8618
    private FamilyInvitation() { }
#pragma warning restore CS8618

    /// <summary>
    /// Factory method to create a new family invitation.
    /// Generates the token hash from the provided plaintext token.
    /// Raises InvitationSentEvent with the plaintext token for the email handler.
    /// </summary>
    /// <param name="familyId">The family to invite to</param>
    /// <param name="invitedByUserId">The user sending the invitation</param>
    /// <param name="inviteeEmail">Email of the person being invited</param>
    /// <param name="role">Role the invitee will have</param>
    /// <param name="tokenHash">SHA256 hash of the plaintext token</param>
    /// <param name="plaintextToken">The plaintext token to include in the email</param>
    public static FamilyInvitation Create(
        FamilyId familyId,
        UserId invitedByUserId,
        Email inviteeEmail,
        FamilyRole role,
        InvitationToken tokenHash,
        string plaintextToken)
    {
        var invitation = new FamilyInvitation
        {
            Id = InvitationId.New(),
            FamilyId = familyId,
            InvitedByUserId = invitedByUserId,
            InviteeEmail = inviteeEmail,
            TokenHash = tokenHash,
            Role = role,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        invitation.RaiseDomainEvent(new InvitationSentEvent(
            invitation.Id,
            invitation.FamilyId,
            invitation.InvitedByUserId,
            invitation.InviteeEmail,
            invitation.Role,
            plaintextToken,
            invitation.ExpiresAt
        ));

        return invitation;
    }

    public FamilyId FamilyId { get; private set; }
    public UserId InvitedByUserId { get; private set; }
    public Email InviteeEmail { get; private set; }
    public InvitationToken TokenHash { get; private set; }
    public FamilyRole Role { get; private set; }
    public InvitationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public UserId? AcceptedByUserId { get; private set; }
    public DateTime? AcceptedAt { get; private set; }

    // Navigation properties
    public Family Family { get; private set; } = null!;
    public User InvitedByUser { get; private set; } = null!;
    public User? AcceptedByUser { get; private set; }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Accept the invitation. Creates the family membership.
    /// </summary>
    public void Accept(UserId userId)
    {
        if (!Status.IsPending())
        {
            throw new DomainException($"Cannot accept invitation in status '{Status.Value}'", DomainErrorCodes.InvitationInvalidStatusForAccept);
        }

        if (IsExpired())
        {
            throw new DomainException("Invitation has expired", DomainErrorCodes.InvitationExpired);
        }

        Status = InvitationStatus.Accepted;
        AcceptedByUserId = userId;
        AcceptedAt = DateTime.UtcNow;

        RaiseDomainEvent(new InvitationAcceptedEvent(
            Id,
            FamilyId,
            userId,
            Role
        ));
    }

    /// <summary>
    /// Decline the invitation.
    /// </summary>
    public void Decline()
    {
        if (!Status.IsPending())
        {
            throw new DomainException($"Cannot decline invitation in status '{Status.Value}'", DomainErrorCodes.InvitationInvalidStatusForDecline);
        }

        Status = InvitationStatus.Declined;

        RaiseDomainEvent(new InvitationDeclinedEvent(Id, FamilyId));
    }

    /// <summary>
    /// Revoke the invitation (by an admin/owner).
    /// </summary>
    public void Revoke()
    {
        if (!Status.IsPending())
        {
            throw new DomainException($"Cannot revoke invitation in status '{Status.Value}'", DomainErrorCodes.InvitationInvalidStatusForRevoke);
        }

        Status = InvitationStatus.Revoked;

        RaiseDomainEvent(new InvitationRevokedEvent(Id, FamilyId));
    }
}
