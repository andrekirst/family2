using HotChocolate.Authorization;

namespace FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;

/// <summary>
/// Root GraphQL mutation type with hierarchical namespace entry points.
/// Mirrors the RootQuery pattern — each method returns a namespace type.
/// </summary>
public class RootMutation
{
    /// <summary>
    /// Family management mutations (create, invite, invitation actions).
    /// </summary>
    [Authorize]
    public FamilyMutation Family() => new();

    /// <summary>
    /// Calendar mutations (create, update, cancel events).
    /// </summary>
    [Authorize]
    public CalendarMutation Calendar() => new();
}
