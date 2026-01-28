namespace FamilyHub.SharedKernel.Presentation.GraphQL.Relay;

/// <summary>
/// Interface for resolving Node types from global IDs.
/// Implementations route global IDs to the appropriate type-specific resolver.
/// </summary>
/// <remarks>
/// <para>
/// The node resolver is the central component that:
/// <list type="bullet">
/// <item><description>Decodes global IDs to extract type information</description></item>
/// <item><description>Routes to the correct type-specific resolver</description></item>
/// <item><description>Returns the entity as an <see cref="INode"/> interface</description></item>
/// </list>
/// </para>
/// <para>
/// Each module registers its type resolvers with the NodeResolver during startup.
/// </para>
/// </remarks>
public interface INodeResolver
{
    /// <summary>
    /// Resolves a node by its global ID.
    /// </summary>
    /// <param name="globalId">The base64-encoded global ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved node, or null if not found.</returns>
    Task<INode?> ResolveAsync(string globalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves multiple nodes by their global IDs.
    /// Optimized for batch loading to prevent N+1 queries.
    /// </summary>
    /// <param name="globalIds">The base64-encoded global IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of resolved nodes (null for IDs that couldn't be resolved).</returns>
    Task<IReadOnlyList<INode?>> ResolveManyAsync(
        IReadOnlyList<string> globalIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a type-specific resolver for a given type name.
    /// </summary>
    /// <typeparam name="T">The node type.</typeparam>
    /// <param name="typeName">The GraphQL type name.</param>
    /// <param name="resolver">The resolver function.</param>
    void RegisterResolver<T>(string typeName, Func<Guid, CancellationToken, Task<T?>> resolver)
        where T : class, INode;
}
