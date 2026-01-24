using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.CancelInvitation;

/// <summary>
/// Command to cancel a pending invitation.
/// Returns Result for parameterless payload mutations.
/// </summary>
/// <param name="InvitationId">The ID of the invitation to cancel.</param>
public record CancelInvitationCommand(
    InvitationId InvitationId
) : ICommand<Result>;
