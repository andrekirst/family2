using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetFamilyMembers;

[ExtendObjectType(typeof(FamilyDto))]
public class QueryType
{
    /// <summary>
    /// Get all members of the current user's family.
    /// </summary>
    [HotChocolate.Types.UsePaging]
    public async Task<List<UserDto>> GetMembers(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetFamilyMembersQuery();

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
