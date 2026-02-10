namespace FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;

/// <summary>
/// Namespace type for current-user queries.
/// Extended by Auth module (profile) and Family module (family, invitations).
/// </summary>
public class MeQuery
{
    /// <summary>
    /// Nested namespace for the current user's invitations.
    /// </summary>
    public MeInvitationsQuery Invitations() => new();
}
