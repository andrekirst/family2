using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for family member invitation operations.
/// </summary>
[ExtendObjectType("Query")]
public sealed class InvitationQueries
{
    /// <summary>
    /// Gets all members of a family.
    /// Requires family membership (any role).
    /// </summary>
    [Authorize]
    [GraphQLDescription("Get all members of a family")]
    public async Task<List<FamilyMemberType>> FamilyMembers(
        Guid familyId,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var users = await userRepository.GetByFamilyIdAsync(FamilyId.From(familyId), cancellationToken);

        return users
            .Where(u => u.DeletedAt == null) // Exclude soft-deleted users
            .Select(u => new FamilyMemberType
            {
                Id = u.Id.Value,
                Email = u.Email.Value,
                EmailVerified = u.EmailVerified,
                Role = MapToGraphQLRole(u.Role),
                JoinedAt = u.CreatedAt,
                IsOwner = u.Role == UserRole.Owner,
                AuditInfo = new FamilyHub.Infrastructure.GraphQL.Types.AuditInfoType
                {
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                }
            })
            .ToList();
    }

    /// <summary>
    /// Gets all pending invitations for a family.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [GraphQLDescription("Get all pending invitations for a family")]
    public async Task<List<PendingInvitationType>> PendingInvitations(
        Guid familyId,
        [Service] IFamilyMemberInvitationRepository invitationRepository,
        CancellationToken cancellationToken)
    {
        var invitations = await invitationRepository.GetPendingByFamilyIdAsync(
            FamilyId.From(familyId),
            cancellationToken);

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
    /// Gets a single invitation by ID.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [GraphQLDescription("Get a single invitation by ID")]
    public async Task<PendingInvitationType?> Invitation(
        Guid invitationId,
        [Service] IFamilyMemberInvitationRepository invitationRepository,
        CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.GetByIdAsync(
            InvitationId.From(invitationId),
            cancellationToken);

        if (invitation == null)
            return null;

        return new PendingInvitationType
        {
            Id = invitation.Id.Value,
            Email = invitation.Email.Value,
            Role = MapToGraphQLRole(invitation.Role),
            Status = MapToGraphQLStatus(invitation.Status),
            InvitedById = invitation.InvitedByUserId.Value,
            InvitedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            Message = invitation.Message,
            DisplayCode = invitation.DisplayCode.Value
        };
    }

    /// <summary>
    /// Gets an invitation by token (for acceptance flow).
    /// No authentication required (public endpoint for invitees).
    /// Returns limited info (no token, no display code).
    /// </summary>
    [GraphQLDescription("Get an invitation by token (public endpoint for accepting invitations)")]
    public async Task<PendingInvitationType?> InvitationByToken(
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
