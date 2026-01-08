using FamilyHub.Infrastructure.GraphQL;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Mappers;

/// <summary>
/// Centralized mapper for User domain entity to GraphQL types.
/// Provides consistent, testable mapping logic for all user-related responses.
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Maps User domain entity to UserType GraphQL output type.
    /// Used in queries (Me) and mutation payloads.
    /// </summary>
    /// <param name="user">User domain entity</param>
    /// <returns>UserType for GraphQL response</returns>
    public static UserType AsGraphQLType(User user)
    {
        return new UserType
        {
            Id = user.Id.Value,
            Email = user.Email.Value,
            EmailVerified = user.EmailVerified,
            AuditInfo = user.AsAuditInfo()
        };
    }
}
