using System.Security.Claims;
using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.GraphQL;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetMyFamily;

[ExtendObjectType(typeof(AuthQueries))]
public class QueryType
{
    /// <summary>
    /// Get the current user's family.
    /// </summary>
    [Authorize]
    public async Task<FamilyDto?> GetMyFamily(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(externalUserIdString))
        {
            return null;
        }

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var query = new GetMyFamilyQuery(externalUserId);

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
