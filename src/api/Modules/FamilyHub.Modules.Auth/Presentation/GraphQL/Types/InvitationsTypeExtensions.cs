using FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken;
using FamilyHub.Modules.Auth.Application.Queries.GetPendingInvitations;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type extensions for InvitationsType.
/// Adds resolver fields for invitation operations.
/// </summary>
[ExtendObjectType(typeof(InvitationsType))]
public sealed class InvitationsTypeExtensions
{
    /// <summary>
    /// Gets all pending invitations for the authenticated user's family.
    /// Requires OWNER or ADMIN role.
    /// User context and authorization are handled by MediatR pipeline behaviors.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")] // Keep for GraphQL layer defense-in-depth
    [GraphQLDescription("Get all pending invitations for the authenticated user's family")]
    public async Task<List<PendingInvitationType>> Pending(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // 1. Create query (no parameters - FamilyId from IUserContext)
        var query = new GetPendingInvitationsQuery();

        // 2. Execute query via MediatR (user context and authorization handled by behaviors)
        var result = await mediator.Send(query, cancellationToken);

        // 3. Map application DTOs → GraphQL types
        return result.Invitations
            .Select(dto => new PendingInvitationType
            {
                Id = dto.Id,
                Email = dto.Email,
                Role = MapToGraphQLRole(dto.Role),
                Status = MapToGraphQLStatus(dto.Status),
                InvitedById = dto.InvitedByUserId,
                InvitedAt = dto.InvitedAt,
                ExpiresAt = dto.ExpiresAt,
                Message = dto.Message,
                DisplayCode = dto.DisplayCode
            })
            .ToList();
    }

    /// <summary>
    /// Gets an invitation by token (for acceptance flow).
    /// No authentication required (public endpoint for invitees).
    /// Returns limited info (no token, no display code for security).
    /// </summary>
    [GraphQLDescription("Get invitation details by token (for invitees to view before accepting)")]
    public async Task<PendingInvitationType?> ByToken(
        string token,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // 1. Create query (convert primitive → Vogen)
        var query = new GetInvitationByTokenQuery(InvitationToken.From(token));

        // 2. Execute query via MediatR
        var result = await mediator.Send(query, cancellationToken);

        // 3. Map application DTO → GraphQL type
        if (result == null)
        {
            return null; // Not found or expired
        }
        return new PendingInvitationType
        {
            Id = result.Id,
            Email = result.Email,
            Role = MapToGraphQLRole(result.Role),
            Status = MapToGraphQLStatus(result.Status),
            InvitedById = result.InvitedByUserId,
            InvitedAt = result.InvitedAt,
            ExpiresAt = result.ExpiresAt,
            Message = result.Message,
            DisplayCode = result.DisplayCode
        };
    }

    /// <summary>
    /// Maps domain FamilyRole to GraphQL UserRoleType.
    /// </summary>
    private static UserRoleType MapToGraphQLRole(FamilyRole domainRole)
    {
        var roleValue = domainRole.Value.ToLowerInvariant();
        return roleValue switch
        {
            "owner" => UserRoleType.OWNER,
            "admin" => UserRoleType.ADMIN,
            "member" => UserRoleType.MEMBER,
            _ => throw new InvalidOperationException($"Unknown role: {roleValue}")
        };
    }

    /// <summary>
    /// Maps domain InvitationStatus to GraphQL InvitationStatusType.
    /// </summary>
    private static InvitationStatusType MapToGraphQLStatus(InvitationStatus domainStatus)
    {
        var statusValue = domainStatus.Value.ToLowerInvariant();
        return statusValue switch
        {
            "pending" => InvitationStatusType.PENDING,
            "accepted" => InvitationStatusType.ACCEPTED,
            "rejected" => InvitationStatusType.REJECTED,
            "canceled" => InvitationStatusType.CANCELLED,
            "expired" => InvitationStatusType.EXPIRED,
            _ => throw new InvalidOperationException($"Unknown invitation status: {statusValue}")
        };
    }
}
