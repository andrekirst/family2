using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Family.Application.Queries;
using FamilyHub.Api.Features.Family.Models;
using FamilyHub.Api.Features.Auth.GraphQL;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Family.GraphQL;

/// <summary>
/// GraphQL queries for family data.
/// Uses CQRS pattern with query bus.
/// </summary>
[ExtendObjectType(typeof(AuthQueries))]
public class FamilyQueries
{
    /// <summary>
    /// Get the current user's family.
    /// </summary>
    [Authorize]
    public async Task<FamilyDto?> GetMyFamily(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(externalUserIdString))
        {
            return null;
        }

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var query = new GetMyFamilyQuery(externalUserId);

        return await queryBus.QueryAsync<FamilyDto?>(query, ct);
    }

    /// <summary>
    /// Get all members of the current user's family.
    /// </summary>
    [Authorize]
    public async Task<List<UserDto>> GetMyFamilyMembers(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var query = new GetFamilyMembersQuery(externalUserId);

        return await queryBus.QueryAsync<List<UserDto>>(query, ct);
    }
}
