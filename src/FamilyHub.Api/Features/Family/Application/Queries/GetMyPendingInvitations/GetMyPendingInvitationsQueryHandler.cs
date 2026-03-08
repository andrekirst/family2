using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetMyPendingInvitations;

/// <summary>
/// Handler for GetMyPendingInvitationsQuery.
/// Retrieves pending invitations for the current user's email address.
/// </summary>
public sealed class GetMyPendingInvitationsQueryHandler(
    IFamilyInvitationRepository invitationRepository,
    IUserRepository userRepository)
    : IQueryHandler<GetMyPendingInvitationsQuery, List<InvitationDto>>
{
    public async ValueTask<List<InvitationDto>> Handle(
        GetMyPendingInvitationsQuery query,
        CancellationToken cancellationToken)
    {
        var user = (await userRepository.GetByIdAsync(query.UserId, cancellationToken))!;

        var invitations = await invitationRepository.GetPendingByEmailAsync(user.Email, cancellationToken);
        return invitations.Select(InvitationMapper.ToDto).ToList();
    }
}
