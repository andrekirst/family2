using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetMyPendingInvitations;

[ExtendObjectType(typeof(MeInvitationsQuery))]
public class QueryType
{
    /// <summary>
    /// Get pending invitations for the current user's email address.
    /// </summary>
    [Authorize]
    [HotChocolate.Types.UsePaging]
    public async Task<List<InvitationDto>> GetPendings(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetMyPendingInvitationsQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
