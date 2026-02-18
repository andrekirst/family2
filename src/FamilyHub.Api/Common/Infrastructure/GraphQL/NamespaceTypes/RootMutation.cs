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
    /// Dashboard mutations (save layout, add/remove/configure widgets).
    /// </summary>
    [Authorize]
    public DashboardMutation Dashboard() => new();

    /// <summary>
    /// Event Chain Engine mutations (create, update, delete, execute chains).
    /// </summary>
    [Authorize]
    public EventChainMutation EventChain() => new();

    /// <summary>
    /// Google Integration mutations (unlink, refresh token).
    /// </summary>
    [Authorize]
    public GoogleIntegrationMutation GoogleIntegration() => new();
}
