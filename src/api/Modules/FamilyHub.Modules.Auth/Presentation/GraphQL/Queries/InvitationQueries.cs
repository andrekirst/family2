using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.Specifications;
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
        // UsersByFamilySpecification already excludes soft-deleted users
        var users = await userRepository.FindAllAsync(
            new UsersByFamilySpecification(FamilyId.From(familyId)),
            cancellationToken);

        return users
            .Select(u => new FamilyMemberType
            {
                Id = u.Id.Value,
                Email = u.Email.Value,
                EmailVerified = u.EmailVerified,
                Role = MapToGraphQlRole(u.Role),
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
    private static UserRoleType MapToGraphQlRole(FamilyRole domainRole)
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
}
