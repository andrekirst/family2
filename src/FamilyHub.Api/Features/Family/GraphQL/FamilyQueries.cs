using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Application.Queries.GetUserById;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Family.GraphQL;

/// <summary>
/// Extends InvitationsQuery with family invitation resolvers.
/// </summary>
[ExtendObjectType(typeof(InvitationsQuery))]
public class InvitationsQueryExtension
{
    /// <summary>
    /// Get pending invitations for the current user's family (admin/family view).
    /// </summary>
    [Authorize]
    public async Task<List<InvitationDto>> GetPendings(
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
    /// Get invitation details by token (public query for the acceptance page).
    /// </summary>
    public async Task<InvitationDto?> GetByToken(
        string token,
        [Service] IFamilyInvitationRepository invitationRepository,
        CancellationToken cancellationToken)
    {
        var tokenHash = SecureTokenHelper.ComputeSha256Hash(token);
        var invitation = await invitationRepository.GetByTokenHashAsync(InvitationToken.From(tokenHash), cancellationToken);

        return invitation is null ? null : InvitationMapper.ToDto(invitation);
    }
}

/// <summary>
/// Extends MeInvitationsQuery with the current user's pending invitations.
/// </summary>
[ExtendObjectType(typeof(MeInvitationsQuery))]
public class MeInvitationsQueryExtension
{
    /// <summary>
    /// Get pending invitations for the current user's email address.
    /// </summary>
    public async Task<List<InvitationDto>> GetPendings(
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
}

/// <summary>
/// Extends FamilyDto with a resolved owner field.
/// Replaces the raw OwnerId GUID with the full UserDto object.
/// </summary>
[ExtendObjectType(typeof(FamilyDto))]
public class FamilyOwnerResolverExtension
{
    public async Task<UserDto?> GetOwner(
        [Parent] FamilyDto parent,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var userId = UserId.From(parent.OwnerId);
        var query = new GetUserByIdQuery(userId);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}

/// <summary>
/// Extends FamilyDto with family member role resolver.
/// </summary>
[ExtendObjectType(typeof(FamilyDto))]
public class FamilyMembersQueryExtension
{
    /// <summary>
    /// Get family members with roles for the current user's family.
    /// </summary>
    public async Task<List<FamilyMemberDto>> GetWithRoles(
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
}
