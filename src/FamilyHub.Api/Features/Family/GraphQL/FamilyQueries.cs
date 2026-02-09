using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.GraphQL;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;
using System.Security.Claims;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;

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
    /// Get pending invitations for the current user's family.
    /// </summary>
    [Authorize]
    public async Task<List<InvitationDto>> GetPendingInvitations(
        ClaimsPrincipal claimsPrincipal,
        [Service] IUserRepository userRepository,
        [Service] IFamilyInvitationRepository invitationRepository,
        [Service] IUserService userService,
        CancellationToken ct)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, ct);

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
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        if (user.FamilyId is null)
        {
            return [];
        }

        var members = await memberRepository.GetByFamilyIdAsync(user.FamilyId.Value, cancellationToken);
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
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        var invitations = await invitationRepository.GetPendingByEmailAsync(user.Email, cancellationToken);
        return invitations.Select(InvitationMapper.ToDto).ToList();
    }

    /// <summary>
    /// Get invitation details by token (public query for the acceptance page).
    /// Returns basic info without sensitive data.
    /// </summary>
    public async Task<InvitationDto?> GetInvitationByToken(
        string token,
        [Service] IFamilyInvitationRepository invitationRepository,
        CancellationToken cancellationToken)
    {
        var tokenHash = SendInvitationCommandHandler.ComputeSha256Hash(token);
        var invitation = await invitationRepository.GetByTokenHashAsync(InvitationToken.From(tokenHash), cancellationToken);

        return invitation is null ? null : InvitationMapper.ToDto(invitation);
    }
}
