using FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;
using FamilyHub.Modules.Family.Presentation.GraphQL.Namespaces;
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
///   auth { ... }      # Authentication operations
///   account { ... }   # Current user's account context
///   family { ... }    # Family operations
///   node(id: ID!)     # Relay Node resolution
///   nodes(ids: [ID!]!) # Batch Node resolution
/// }
/// </code>
/// </para>
/// <para>
/// The namespace containers are empty records. Actual fields are added
/// via separate type extensions in each module.
/// </para>
/// </remarks>
[ExtendObjectType("Query")]
public sealed class RootQueryExtensions
{
    /// <summary>
    /// Entry point for authentication-related queries.
    /// </summary>
    /// <returns>The auth namespace container.</returns>
    public AuthQueries Auth() => new();

    /// <summary>
    /// Entry point for account-related queries (current user context).
    /// Requires authentication.
    /// </summary>
    /// <returns>The account namespace container.</returns>
    [Authorize]
    public AccountQueries Account() => new();

    /// <summary>
    /// Entry point for family-related queries.
    /// Requires authentication.
    /// </summary>
    /// <returns>The family namespace container.</returns>
    [Authorize]
    public FamilyQueries Family() => new();

    /// <summary>
    /// Resolves a single node by its global ID.
    /// Implements Relay Node specification.
    /// </summary>
    /// <param name="id">The global ID to resolve.</param>
    /// <param name="resolver">The node resolver service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved node, or null if not found.</returns>
    public async Task<IRelayNode?> Node(
        [ID] string id,
        [Service] INodeResolver resolver,
        CancellationToken cancellationToken)
    {
        return await resolver.ResolveAsync(id, cancellationToken);
    }

    /// <summary>
    /// Resolves multiple nodes by their global IDs.
    /// Implements Relay batch Node resolution.
    /// </summary>
    /// <param name="ids">The global IDs to resolve.</param>
    /// <param name="resolver">The node resolver service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of resolved nodes (null for IDs that couldn't be resolved).</returns>
    public async Task<IReadOnlyList<IRelayNode?>> Nodes(
        [ID] IReadOnlyList<string> ids,
        [Service] INodeResolver resolver,
        CancellationToken cancellationToken)
    {
        return await resolver.ResolveManyAsync(ids, cancellationToken);
    }
}
