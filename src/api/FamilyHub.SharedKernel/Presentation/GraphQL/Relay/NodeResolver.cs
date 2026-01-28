using System.Collections.Concurrent;

namespace FamilyHub.SharedKernel.Presentation.GraphQL.Relay;

/// <summary>
/// Default implementation of <see cref="INodeResolver"/>.
/// Routes global IDs to type-specific resolvers registered by each module.
/// </summary>
/// <remarks>
/// <para>
/// Registration happens during application startup when each module calls
/// <see cref="RegisterResolver{T}"/> for its entity types.
/// </para>
/// <para>
/// Example registration in a module's service registration:
/// <code>
/// services.AddSingleton&lt;INodeResolver&gt;(sp =>
/// {
///     var resolver = new NodeResolver();
///     resolver.RegisterResolver&lt;User&gt;("User", async (id, ct) =>
///         await sp.GetRequiredService&lt;IUserRepository&gt;().GetByIdAsync(UserId.From(id), ct));
///     return resolver;
/// });
/// </code>
/// </para>
/// </remarks>
public sealed class NodeResolver : INodeResolver
{
    private readonly ConcurrentDictionary<string, Func<Guid, CancellationToken, Task<INode?>>> _resolvers = new();

    /// <inheritdoc />
    public async Task<INode?> ResolveAsync(string globalId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(globalId))
        {
            return null;
        }

        if (!GlobalIdSerializer.TryDeserialize(globalId, out var parsed))
        {
            return null;
        }

        if (!_resolvers.TryGetValue(parsed.TypeName, out var resolver))
        {
            return null;
        }

        return await resolver(parsed.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<INode?>> ResolveManyAsync(
        IReadOnlyList<string> globalIds,
        CancellationToken cancellationToken = default)
    {
        if (globalIds is null || globalIds.Count == 0)
        {
            return Array.Empty<INode?>();
        }

        // Group by type for batch loading optimization
        var byType = globalIds
            .Select((id, index) => (id, index, parsed: GlobalIdSerializer.TryDeserialize(id, out var p) ? p : default))
            .Where(x => x.parsed != default)
            .GroupBy(x => x.parsed.TypeName);

        var results = new INode?[globalIds.Count];

        foreach (var group in byType)
        {
            if (!_resolvers.TryGetValue(group.Key, out var resolver))
            {
                continue;
            }

            // Resolve each in the group (could be optimized with batch loaders)
            foreach (var item in group)
            {
                results[item.index] = await resolver(item.parsed.Id, cancellationToken);
            }
        }

        return results;
    }

    /// <inheritdoc />
    public void RegisterResolver<T>(string typeName, Func<Guid, CancellationToken, Task<T?>> resolver)
        where T : class, INode
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentNullException.ThrowIfNull(resolver);

        _resolvers[typeName] = async (id, ct) => await resolver(id, ct);
    }

    /// <summary>
    /// Gets the registered type names for debugging/introspection.
    /// </summary>
    public IReadOnlyCollection<string> RegisteredTypes => _resolvers.Keys.ToList().AsReadOnly();
}
