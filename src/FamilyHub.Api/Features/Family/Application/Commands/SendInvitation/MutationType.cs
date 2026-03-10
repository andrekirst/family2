using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;

[ExtendObjectType(typeof(FamilyMutation))]
public class MutationType
{
    /// <summary>
    /// Send a family invitation to an email address.
    /// Requires Owner or Admin role in the family.
    /// </summary>
    [Authorize]
    public async Task<InvitationDto> Invite(
        SendInvitationRequest input,
        [Service] ICommandBus commandBus,
        [Service] IFamilyInvitationRepository invitationRepository,
        CancellationToken cancellationToken)
    {
        var command = new SendInvitationCommand(
            Email.From(input.Email.Trim()),
            FamilyRole.From(input.Role));

        var result = await commandBus.SendAsync(command, cancellationToken);

        var invitation = await invitationRepository.GetByIdAsync(result.InvitationId, cancellationToken);
        return InvitationMapper.ToDto(invitation!);
    }
}
