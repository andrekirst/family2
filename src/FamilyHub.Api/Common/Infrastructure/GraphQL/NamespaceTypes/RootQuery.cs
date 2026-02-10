using HotChocolate.Authorization;

namespace FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;

/// <summary>
/// Root GraphQL query type with hierarchical namespace entry points.
/// Each method returns a namespace type that groups related queries by domain.
/// </summary>
public class RootQuery
{
    /// <summary>
    /// Queries scoped to the current authenticated user.
    /// </summary>
    [Authorize]
    public MeQuery Me() => new();

    /// <summary>
    /// Invitation queries (mixed auth: byToken is public, pendings requires auth).
    /// </summary>
    public InvitationsQuery Invitations() => new();

    /// <summary>
    /// User lookup queries.
    /// </summary>
    [Authorize]
    public UsersQuery Users() => new();
}
