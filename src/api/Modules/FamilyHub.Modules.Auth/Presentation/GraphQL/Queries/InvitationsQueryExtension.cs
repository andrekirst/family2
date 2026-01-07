using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;

/// <summary>
/// HotChocolate Query extension that adds the 'invitations' namespace to the root Query type.
/// Provides access to family member invitation operations.
/// </summary>
[ExtendObjectType("Query")]
public sealed class InvitationsQueryExtension
{
    /// <summary>
    /// Accesses invitation operations namespace.
    /// Fields are resolved via InvitationsTypeExtensions.
    /// </summary>
    [GraphQLDescription("Family member invitation operations")]
    public InvitationsType Invitations() => new InvitationsType();
}
