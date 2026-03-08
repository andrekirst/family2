using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetPendingInvitations;

/// <summary>
/// Handler for GetPendingInvitationsQuery.
/// Retrieves pending invitations for the current user's family.
/// </summary>
public sealed class GetPendingInvitationsQueryHandler(
    IFamilyInvitationRepository invitationRepository)
    : IQueryHandler<GetPendingInvitationsQuery, List<InvitationDto>>
{
    public async ValueTask<List<InvitationDto>> Handle(
        GetPendingInvitationsQuery query,
        CancellationToken cancellationToken)
    {
        var invitations = await invitationRepository.GetPendingByFamilyIdAsync(query.FamilyId, cancellationToken);
        return invitations.Select(InvitationMapper.ToDto).ToList();
    }
}
