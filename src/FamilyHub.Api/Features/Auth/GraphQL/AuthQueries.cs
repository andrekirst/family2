using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Application.Queries;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Models;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Auth.GraphQL;

/// <summary>
/// GraphQL queries for authentication and user data.
/// Uses CQRS pattern with query bus.
/// </summary>
public class AuthQueries
{
    /// <summary>
    /// Get the currently authenticated user's profile.
    /// </summary>
    [Authorize]
    public async Task<UserDto?> GetCurrentUser(
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

    /// <summary>
    /// Get user by ID (admin only - for now just authenticated).
    /// </summary>
    [Authorize]
    public async Task<UserDto?> GetUserById(
        Guid userId,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var userIdVo = UserId.From(userId);
        var query = new GetUserByIdQuery(userIdVo);

        return await queryBus.QueryAsync<UserDto?>(query, ct);
    }
}
