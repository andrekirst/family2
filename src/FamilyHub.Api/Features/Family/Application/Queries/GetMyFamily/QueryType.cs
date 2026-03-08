using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetMyFamily;

[ExtendObjectType(typeof(MeQuery))]
public class QueryType
{
    /// <summary>
    /// Get the current user's family.
    /// </summary>
    public async Task<FamilyDto?> GetFamily(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(externalUserIdString))
        {
            return null;
        }

        var externalUserId = ExternalUserId.From(externalUserIdString);

        var user = await userRepository.GetByExternalIdAsync(externalUserId, cancellationToken);
        if (user?.FamilyId is null)
        {
            return null;
        }

        var query = new GetMyFamilyQuery(externalUserId, user.FamilyId.Value);

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
