using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type extensions for InvitationsType.
/// Adds resolver fields for invitation operations.
/// </summary>
[ExtendObjectType(typeof(InvitationsType))]
public sealed class InvitationsTypeExtensions
{
    /// <summary>
    /// Gets all pending invitations for a family.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [GraphQLDescription("Get all pending invitations for the authenticated user's family")]
    public async Task<List<PendingInvitationType>> Pending(
        [Service] ICurrentUserService currentUserService,
        [Service] IUserRepository userRepository,
        [Service] IFamilyMemberInvitationRepository invitationRepository,
        CancellationToken cancellationToken)
    {
        // 1. Get authenticated user's ID from JWT token
        var userId = await currentUserService.GetUserIdAsync(cancellationToken);

        // 2. Fetch user entity to get their FamilyId
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException(
                $"Authenticated user with ID {userId.Value} not found in database");
        }

        // 3. Get invitations for USER'S family (not client-provided)
        var invitations = await invitationRepository.GetPendingByFamilyIdAsync(
            user.FamilyId,
            cancellationToken);

        // 4. Map and return
        return invitations.Select(i => new PendingInvitationType
        {
            Id = i.Id.Value,
            Email = i.Email.Value,
            Role = MapToGraphQLRole(i.Role),
            Status = MapToGraphQLStatus(i.Status),
            InvitedById = i.InvitedByUserId.Value,
            InvitedAt = i.CreatedAt,
            ExpiresAt = i.ExpiresAt,
            Message = i.Message,
            DisplayCode = i.DisplayCode.Value
        }).ToList();
    }

    /// <summary>
    /// Gets an invitation by token (for acceptance flow).
    /// No authentication required (public endpoint for invitees).
    /// Returns limited info (no token, no display code for security).
    /// </summary>
    [GraphQLDescription("Get an invitation by token (public endpoint for accepting invitations)")]
    public async Task<PendingInvitationType?> ByToken(
        string token,
        [Service] IFamilyMemberInvitationRepository invitationRepository,
        CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.GetByTokenAsync(
            InvitationToken.From(token),
            cancellationToken);

        if (invitation == null)
            return null;

        // Return limited info for public query (no display code for security)
        return new PendingInvitationType
        {
            Id = invitation.Id.Value,
            Email = invitation.Email.Value,
            Role = MapToGraphQLRole(invitation.Role),
            Status = MapToGraphQLStatus(invitation.Status),
            ExpiresAt = invitation.ExpiresAt,
            InvitedAt = invitation.CreatedAt,
            Message = invitation.Message
            // DisplayCode intentionally excluded for security
        };
    }

    /// <summary>
    /// Maps domain UserRole to GraphQL UserRoleType.
    /// </summary>
    private static UserRoleType MapToGraphQLRole(UserRole domainRole)
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
