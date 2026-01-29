using FamilyHub.Api.GraphQL.Namespaces;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;
using FamilyHub.SharedKernel.Presentation.GraphQL.Relay;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using IRelayNode = FamilyHub.SharedKernel.Presentation.GraphQL.Relay.INode;

namespace FamilyHub.Api.GraphQL;

/// <summary>
/// Extends the root Query type with namespace entry points.
/// </summary>
/// <remarks>
/// <para>
/// This extension creates the nested namespace structure:
/// <code>
/// query {
///   auth { ... }      # Authentication operations (public - login, register, password reset)
///   me { ... }        # User-centric queries (profile, family, invitations)
///   health { ... }    # Health check endpoints
///   node(id: ID!)     # Relay Node resolution
///   nodes(ids: [ID!]!) # Batch Node resolution
/// }
/// </code>
/// </para>
/// <para>
/// The namespace containers are empty records. Actual fields are added
/// via separate type extensions in each module.
/// </para>
/// <para>
/// <strong>Migration Note:</strong> The previous <c>account</c> and <c>family</c>
/// namespaces have been consolidated into <c>me</c> for a more user-centric API design.
/// </para>
/// </remarks>
[ExtendObjectType("Query")]
public sealed class RootQueryExtensions
{
    /// <summary>
    /// Entry point for authentication-related queries.
    /// Public operations: login, register, password reset, etc.
    /// </summary>
    /// <returns>The auth namespace container.</returns>
    [GraphQLDescription("Authentication operations (public - login, register, password reset).")]
    public AuthQueries Auth() => new();

    /// <summary>
    /// Entry point for user-centric queries.
    /// Consolidates profile, family, and invitation queries under one namespace.
    /// Requires authentication.
    /// </summary>
    /// <returns>The me namespace container.</returns>
    [Authorize]
    [GraphQLDescription("User-centric queries for profile, family, and invitations.")]
    public MeQueries Me() => new();

    /// <summary>
    /// Entry point for health check queries.
    /// Liveness is public, detailed health requires authentication.
    /// </summary>
    /// <returns>The health namespace container.</returns>
    [GraphQLDescription("Health check endpoints (liveness is public, details require auth).")]
    public HealthQueries Health() => new();

    // NOTE: Relay Node queries temporarily disabled due to HotChocolate IdField expression validation
    // issue with Vogen value objects. The pattern .IdField(x => x.Id.Value) is rejected as
    // "not a property-expression or method-call-expression". Without registered Node types,
    // the projection interceptor fails. Fix tracked in issue #XXX.
    //
    // To re-enable:
    // 1. Fix the ImplementsNode() issue in ObjectTypes
    // 2. Uncomment the methods below
    // 3. Relay clients can still use global IDs from 'id' fields on types

    // /// <summary>
    // /// Resolves a single node by its global ID.
    // /// Implements Relay Node specification.
    // /// </summary>
    // /// <param name="id">The global ID to resolve.</param>
    // /// <param name="resolver">The node resolver service.</param>
    // /// <param name="cancellationToken">Cancellation token.</param>
    // /// <returns>The resolved node, or null if not found.</returns>
    // public async Task<IRelayNode?> Node(
    //     [ID] string id,
    //     [Service] INodeResolver resolver,
    //     CancellationToken cancellationToken)
    // {
    //     return await resolver.ResolveAsync(id, cancellationToken);
    // }

    // /// <summary>
    // /// Resolves multiple nodes by their global IDs.
    // /// Implements Relay batch Node resolution.
    // /// </summary>
    // /// <param name="ids">The global IDs to resolve.</param>
    // /// <param name="resolver">The node resolver service.</param>
    // /// <param name="cancellationToken">Cancellation token.</param>
    // /// <returns>A list of resolved nodes (null for IDs that couldn't be resolved).</returns>
    // public async Task<IReadOnlyList<IRelayNode?>> Nodes(
    //     [ID] IReadOnlyList<string> ids,
    //     [Service] INodeResolver resolver,
    //     CancellationToken cancellationToken)
    // {
    //     return await resolver.ResolveManyAsync(ids, cancellationToken);
    // }
}
