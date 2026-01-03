using FamilyHub.Infrastructure.GraphQL.Types;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Adapters;

/// <summary>
/// Adapter for mapping User entity to UserType (GraphQL output).
/// Used for "Me" query and other user-related GraphQL responses.
/// </summary>
public static class UserAuthenticationAdapter
{
    /// <summary>
    /// Maps User entity to UserType for GraphQL response.
    /// </summary>
    /// <param name="user">User domain entity</param>
    /// <returns>UserType for GraphQL response</returns>
    /// <summary>
    /// Maps User entity to UserType for GraphQL response.
    /// </summary>
    /// <param name="user">User domain entity</param>
    /// <returns>UserType for GraphQL response</returns>
    public static UserType ToGraphQLType(User user)
    {
        return new UserType
        {
            Id = user.Id.Value,
            Email = user.Email.Value,
            EmailVerified = user.EmailVerified,
            FamilyId = user.FamilyId.Value,
            AuditInfo = new AuditInfoType
            {
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            }
        };
    }
}
