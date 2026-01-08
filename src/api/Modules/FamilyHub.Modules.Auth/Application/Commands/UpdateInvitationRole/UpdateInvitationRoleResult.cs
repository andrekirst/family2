using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.UpdateInvitationRole;

/// <summary>
/// Result returned after successfully updating an invitation's role.
/// Contains the updated invitation information.
/// </summary>
public record UpdateInvitationRoleResult
{
    public required InvitationId InvitationId { get; init; }
    public required FamilyRole Role { get; init; }
}
