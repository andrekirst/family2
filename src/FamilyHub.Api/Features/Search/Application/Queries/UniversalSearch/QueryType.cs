using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Search.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Search.Application.Queries.UniversalSearch;

[ExtendObjectType(typeof(SearchQuery))]
public class QueryType
{
    [Authorize]
    public async Task<UniversalSearchResult> Universal(
        UniversalSearchRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        [Service] IUserRepository userRepository,
        [Service] IFamilyMemberRepository familyMemberRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
                                   ?? throw new UnauthorizedAccessException("User not authenticated");

        var user = await userRepository.GetByExternalIdAsync(
            ExternalUserId.From(externalUserIdString), cancellationToken);
        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        // Resolve permissions from family membership
        string[]? permissions = null;
        if (user.FamilyId is not null)
        {
            var member = await familyMemberRepository.GetByUserAndFamilyAsync(
                user.Id, user.FamilyId.Value, cancellationToken);
            if (member is not null)
            {
                permissions = member.Role.GetPermissions().ToArray();
            }
        }

        var query = new UniversalSearchQuery(
            UserId: user.Id,
            FamilyId: user.FamilyId,
            Query: input.Query,
            Modules: input.Modules,
            Limit: input.Limit ?? 10,
            UserPermissions: permissions,
            Locale: input.Locale);

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
