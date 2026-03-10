using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetFamilyMembersWithRoles;

[ExtendObjectType(typeof(FamilyDto))]
public class QueryType
{
    /// <summary>
    /// Get family members with roles for the current user's family.
    /// </summary>
    [Authorize]
    public async Task<List<FamilyMemberDto>> GetWithRoles(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetFamilyMembersWithRolesQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
