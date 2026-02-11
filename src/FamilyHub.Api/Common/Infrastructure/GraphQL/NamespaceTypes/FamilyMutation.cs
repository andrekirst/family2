using HotChocolate.Authorization;

namespace FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;

/// <summary>
/// Namespace type for family mutations.
/// Extended by CreateFamily and SendInvitation mutation types.
/// </summary>
public class FamilyMutation
{
    /// <summary>
    /// Invitation actions namespace. Pass an ID for ID-based operations (accept, decline, revoke).
    /// Token-based operations ignore the ID parameter.
    /// </summary>
    [Authorize]
    public FamilyInvitationMutation Invitation(Guid? id = null) => new(id);

    /// <summary>
    /// Calendar mutations namespace (create, update, cancel events).
    /// </summary>
    [Authorize]
    public FamilyCalendarMutation Calendar() => new();
}
