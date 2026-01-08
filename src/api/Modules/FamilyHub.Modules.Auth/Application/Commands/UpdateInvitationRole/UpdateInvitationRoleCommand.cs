using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.UpdateInvitationRole;

/// <summary>
/// Command to update the role of a pending invitation.
/// Requires Owner or Admin role.
/// User context and authorization are handled by pipeline behaviors.
/// </summary>
public record UpdateInvitationRoleCommand(
    InvitationId InvitationId,
    FamilyRole NewRole
) : IRequest<FamilyHub.SharedKernel.Domain.Result<UpdateInvitationRoleResult>>,
    IRequireAuthentication,
    IRequireFamilyContext,
    IRequireOwnerOrAdminRole;
