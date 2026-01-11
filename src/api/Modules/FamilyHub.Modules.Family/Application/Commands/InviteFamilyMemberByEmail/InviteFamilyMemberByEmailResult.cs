using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Application.Commands.InviteFamilyMemberByEmail;

/// <summary>
/// Result of inviting a family member by email.
/// Contains invitation information returned from the command handler.
/// </summary>
public record InviteFamilyMemberByEmailResult
{
    /// <summary>
    /// Gets the unique identifier for the invitation.
    /// </summary>
    public required InvitationId InvitationId { get; init; }

    /// <summary>
    /// Gets the email address of the invited user.
    /// </summary>
    public required Email Email { get; init; }

    /// <summary>
    /// Gets the role assigned to the invited family member.
    /// </summary>
    public required FamilyRole Role { get; init; }

    /// <summary>
    /// Gets the invitation token used to accept the invitation.
    /// </summary>
    public required InvitationToken Token { get; init; }

    /// <summary>
    /// Gets the human-readable display code for the invitation.
    /// </summary>
    public required InvitationDisplayCode DisplayCode { get; init; }

    /// <summary>
    /// Gets the expiration date and time of the invitation.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Gets the current status of the invitation.
    /// </summary>
    public required InvitationStatus Status { get; init; }
}
