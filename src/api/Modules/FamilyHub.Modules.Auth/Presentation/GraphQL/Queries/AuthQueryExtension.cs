using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;

/// <summary>
/// HotChocolate Query extension that adds the 'auth' namespace to the root Query type.
/// Provides access to authentication-related operations.
/// </summary>
[ExtendObjectType("Query")]
public sealed class AuthQueryExtension
{
    /// <summary>
    /// Accesses authentication operations namespace.
    /// Fields are resolved via AuthTypeExtensions.
    /// </summary>
    [GraphQLDescription("Authentication operations")]
    public AuthType Auth() => new AuthType();
}
