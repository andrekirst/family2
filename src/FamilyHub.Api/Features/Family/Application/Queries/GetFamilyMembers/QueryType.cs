using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
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
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
                                   ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);

        var user = await userRepository.GetByExternalIdAsync(externalUserId, cancellationToken);
        if (user?.FamilyId is null)
        {
            return [];
        }

        var query = new GetFamilyMembersQuery(externalUserId, user.FamilyId.Value);

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
