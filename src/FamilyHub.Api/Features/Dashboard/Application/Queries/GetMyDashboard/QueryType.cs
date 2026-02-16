using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetMyDashboard;

[ExtendObjectType(typeof(DashboardQuery))]
public class QueryType
{
    [Authorize]
    public async Task<DashboardLayoutDto?> MyDashboard(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(externalUserIdString))
            return null;

        var user = await userRepository.GetByExternalIdAsync(
            ExternalUserId.From(externalUserIdString), cancellationToken);
        if (user is null)
            return null;

        var query = new GetMyDashboardQuery(user.Id);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
