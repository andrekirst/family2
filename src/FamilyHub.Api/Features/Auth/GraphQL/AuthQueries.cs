using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Auth.Application.Queries.GetCurrentUser;
using FamilyHub.Api.Features.Auth.Application.Queries.GetUserById;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Auth.GraphQL;

/// <summary>
/// Extends MeQuery with the current user's profile resolver.
/// </summary>
[ExtendObjectType(typeof(MeQuery))]
public class MeProfileQueryExtension
{
    /// <summary>
    /// Get the currently authenticated user's profile.
    /// </summary>
    public async Task<UserDto?> GetProfile(
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
        var query = new GetCurrentUserQuery(externalUserId);

        return await queryBus.QueryAsync<UserDto?>(query, ct);
    }
}

/// <summary>
/// Extends UsersQuery with user lookup by ID.
/// </summary>
[ExtendObjectType(typeof(UsersQuery))]
public class UsersQueryExtension
{
    /// <summary>
    /// Get user by ID.
    /// </summary>
    public async Task<UserDto?> GetById(
        Guid userId,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var userIdVo = UserId.From(userId);
        var query = new GetUserByIdQuery(userIdVo);

        return await queryBus.QueryAsync<UserDto?>(query, ct);
    }
}
