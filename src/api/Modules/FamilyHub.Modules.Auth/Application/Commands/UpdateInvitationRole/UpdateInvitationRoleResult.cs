using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.UpdateInvitationRole;

/// <summary>
/// Result returned after successfully updating an invitation's role.
/// Contains the updated invitation information.
/// </summary>
public record UpdateInvitationRoleResult
{
    /// <summary>
    /// Gets the unique identifier of the updated invitation.
    /// </summary>
    public required InvitationId InvitationId { get; init; }

    /// <summary>
    /// Gets the newly assigned role for the invitation.
    /// </summary>
    public required FamilyRole Role { get; init; }
}
