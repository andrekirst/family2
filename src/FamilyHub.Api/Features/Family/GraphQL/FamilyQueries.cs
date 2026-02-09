using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.GraphQL;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Family.Application.Handlers;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Application.Queries;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Family.GraphQL;

/// <summary>
/// GraphQL queries for family data.
/// Uses CQRS pattern with query bus.
/// Extends AuthQueries (the root query type).
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

    /// <summary>
    /// Get pending invitations for the current user's family.
    /// </summary>
    [Authorize]
    public async Task<List<InvitationDto>> GetPendingInvitations(
        ClaimsPrincipal claimsPrincipal,
        [Service] IUserRepository userRepository,
        [Service] IFamilyInvitationRepository invitationRepository,
        CancellationToken ct)
    {
        var user = await GetCurrentUser(claimsPrincipal, userRepository, ct);

        if (user.FamilyId is null)
        {
            return [];
        }

        var invitations = await invitationRepository.GetPendingByFamilyIdAsync(user.FamilyId.Value, ct);
        return invitations.Select(InvitationMapper.ToDto).ToList();
    }

    /// <summary>
    /// Get family members with roles for the current user's family.
    /// </summary>
    [Authorize]
    public async Task<List<FamilyMemberDto>> GetFamilyMembersWithRoles(
        ClaimsPrincipal claimsPrincipal,
        [Service] IUserRepository userRepository,
        [Service] IFamilyMemberRepository memberRepository,
        CancellationToken ct)
    {
        var user = await GetCurrentUser(claimsPrincipal, userRepository, ct);

        if (user.FamilyId is null)
        {
            return [];
        }

        var members = await memberRepository.GetByFamilyIdAsync(user.FamilyId.Value, ct);
        return members.Select(FamilyMemberMapper.ToDto).ToList();
    }

    /// <summary>
    /// Get pending invitations for the current user's email address.
    /// Used by the dashboard to show invitations the user can accept/decline.
    /// </summary>
    [Authorize]
    public async Task<List<InvitationDto>> GetMyPendingInvitations(
        ClaimsPrincipal claimsPrincipal,
        [Service] IUserRepository userRepository,
        [Service] IFamilyInvitationRepository invitationRepository,
        CancellationToken ct)
    {
        var user = await GetCurrentUser(claimsPrincipal, userRepository, ct);

        var invitations = await invitationRepository.GetPendingByEmailAsync(user.Email, ct);
        return invitations.Select(InvitationMapper.ToDto).ToList();
    }

    /// <summary>
    /// Get invitation details by token (public query for the acceptance page).
    /// Returns basic info without sensitive data.
    /// </summary>
    public async Task<InvitationDto?> GetInvitationByToken(
        string token,
        [Service] IFamilyInvitationRepository invitationRepository,
        CancellationToken ct)
    {
        var tokenHash = SendInvitationCommandHandler.ComputeSha256Hash(token);
        var invitation = await invitationRepository.GetByTokenHashAsync(InvitationToken.From(tokenHash), ct);

        if (invitation is null)
        {
            return null;
        }

        return InvitationMapper.ToDto(invitation);
    }

    private static async Task<Auth.Domain.Entities.User> GetCurrentUser(
        ClaimsPrincipal claimsPrincipal,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        return await userRepository.GetByExternalIdAsync(externalUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");
    }
}
