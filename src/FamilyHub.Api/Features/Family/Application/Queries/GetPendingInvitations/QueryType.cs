using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetPendingInvitations;

[ExtendObjectType(typeof(InvitationsQuery))]
public class QueryType
{
    /// <summary>
    /// Get pending invitations for the current user's family (admin/family view).
    /// </summary>
    [Authorize]
    public async Task<List<InvitationDto>> GetPendings(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetPendingInvitationsQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
