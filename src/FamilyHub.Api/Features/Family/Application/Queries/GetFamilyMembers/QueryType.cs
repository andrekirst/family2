using System.Security.Claims;
using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetFamilyMembers;

[ExtendObjectType(typeof(FamilyDto))]
public class QueryType
{
    /// <summary>
    /// Get all members of the current user's family.
    /// </summary>
    public async Task<List<UserDto>> GetMembers(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
                                   ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var query = new GetFamilyMembersQuery(externalUserId);

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
