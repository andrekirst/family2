using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitationById;

[ExtendObjectType(typeof(FamilyInvitationMutation))]
public class MutationType
{
    /// <summary>
    /// Accept a family invitation by ID (from the dashboard, without the email token).
    /// Verifies that the accepting user's email matches the invitation's invitee email.
    /// </summary>
    [Authorize]
    public async Task<AcceptInvitationResultDto> Accept(
        [Parent] FamilyInvitationMutation parent,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var invitationId = parent.InvitationId.HasValue
            ? InvitationId.From(parent.InvitationId.Value)
            : (InvitationId?)null;

        var command = new AcceptInvitationByIdCommand(invitationId);
        var result = await commandBus.SendAsync(command, cancellationToken);

        return new AcceptInvitationResultDto
        {
            FamilyId = result.FamilyId.Value,
            FamilyMemberId = result.FamilyMemberId.Value,
            Success = true
        };
    }
}
