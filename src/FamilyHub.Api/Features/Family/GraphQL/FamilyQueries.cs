using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Api.Features.Auth.Application.Queries.GetUserById;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.GraphQL;

/// <summary>
/// Extends InvitationsQuery with the by-token resolver (public, no auth needed).
/// </summary>
[ExtendObjectType(typeof(InvitationsQuery))]
public class InvitationsQueryExtension
{
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
        var query = new GetUserByIdQuery(TargetUserId: userId);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
