using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;

/// <summary>
/// Result of accepting an invitation.
/// Contains the family information the user has joined.
/// </summary>
public record AcceptInvitationResult
{
    /// <summary>
    /// The family ID the user joined.
    /// </summary>
    public required FamilyId FamilyId { get; init; }

    /// <summary>
    /// The family name.
    /// </summary>
    public required FamilyName FamilyName { get; init; }

    /// <summary>
    /// The role assigned to the user.
    /// </summary>
    public required FamilyRole Role { get; init; }
}
