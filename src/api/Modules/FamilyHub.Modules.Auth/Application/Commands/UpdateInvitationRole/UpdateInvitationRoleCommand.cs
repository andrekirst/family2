using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Commands.UpdateInvitationRole;

/// <summary>
/// Command to update the role of a pending invitation.
/// Only OWNER and ADMIN can execute this command.
/// </summary>
public record UpdateInvitationRoleCommand(
    InvitationId InvitationId,
    UserRole NewRole
) : IRequest<FamilyHub.SharedKernel.Domain.Result<UpdateInvitationRoleResult>>;
