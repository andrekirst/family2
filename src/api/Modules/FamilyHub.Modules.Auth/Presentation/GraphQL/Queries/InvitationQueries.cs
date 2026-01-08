using FamilyDomain = FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.ValueObjects;
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
                IsOwner = u.Role == FamilyRole.Owner,
                AuditInfo = new FamilyHub.Infrastructure.GraphQL.Types.AuditInfoType
                {
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                }
            })
            .ToList();
    }

    // NOTE: PendingInvitations, Invitation, and InvitationByToken have been moved to
    // InvitationsTypeExtensions as part of the schema restructuring (invitations.pending, invitations.byToken)

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
