using System.Security.Claims;
using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
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
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IFamilyInvitationRepository invitationRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException("You must be part of a family to send invitations");
        }

        var command = new SendInvitationCommand(
            user.FamilyId.Value,
            user.Id,
            Email.From(input.Email.Trim()),
            FamilyRole.From(input.Role));

        var result = await commandBus.SendAsync(command, cancellationToken);

        var invitation = await invitationRepository.GetByIdAsync(result.InvitationId, cancellationToken);
        return InvitationMapper.ToDto(invitation!);
    }
}
