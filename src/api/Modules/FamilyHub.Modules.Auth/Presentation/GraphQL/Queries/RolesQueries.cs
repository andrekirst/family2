using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using HotChocolate.Authorization;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for user role reference data.
/// Moved from references.roles to root level for better discoverability.
/// </summary>
[ExtendObjectType("Query")]
public sealed class RolesQueries
{
    /// <summary>
    /// Gets user roles with metadata (labels, descriptions, UI styling).
    /// </summary>
    [Authorize]
    [GraphQLDescription("User roles with metadata")]
    public RolesType Roles()
    {
        var allRoles = GetAllRoles();

        return new RolesType
        {
            All = allRoles,
            Invitable = allRoles
                .Where(r => r.Value != UserRoleType.OWNER)
                .ToList()
                .AsReadOnly()
        };
    }

    /// <summary>
    /// Returns all available user roles with metadata.
    /// </summary>
    private static IReadOnlyList<RoleMetadata> GetAllRoles()
    {
        return new List<RoleMetadata>
        {
            new()
            {
                Value = UserRoleType.OWNER,
                Label = "Owner",
                Description = "Full control over family settings, members, and data. Can delete family and transfer ownership.",
                BadgeColorClass = "bg-purple-100 text-purple-800"
            },
            new()
            {
                Value = UserRoleType.ADMIN,
                Label = "Admin",
                Description = "Can manage family settings, invite members, and assign roles. Cannot delete family or change ownership.",
                BadgeColorClass = "bg-blue-100 text-blue-800"
            },
            new()
            {
                Value = UserRoleType.MEMBER,
                Label = "Member",
                Description = "Can view and contribute to family content. Cannot manage settings or invite members.",
                BadgeColorClass = "bg-green-100 text-green-800"
            }
        }.AsReadOnly();
    }
}
