using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;
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
) : ICommand<FamilyHub.SharedKernel.Domain.Result<UpdateInvitationRoleResult>>,
    IRequireFamilyContext,
    IRequireOwnerOrAdminRole;
