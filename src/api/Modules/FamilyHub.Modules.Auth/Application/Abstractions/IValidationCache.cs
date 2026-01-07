namespace FamilyHub.Modules.Auth.Application.Abstractions;

/// <summary>
/// Scoped service for caching entities fetched during validation.
/// Eliminates duplicate database queries between validators and handlers.
/// Lifetime: Scoped (per HTTP request) - automatically disposed at end of request.
/// </summary>
/// <remarks>
/// Cache keys should follow the pattern: "{EntityType}:{EntityId}"
/// Examples:
/// - "FamilyMemberInvitation:{token-value}"
/// - "Family:{familyId-guid}"
/// - "User:{userId-guid}"
/// </remarks>
public interface IValidationCache
{
    /// <summary>
    /// Stores an entity in the cache with the specified key.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity to cache (must be reference type).</typeparam>
    /// <param name="key">Cache key (recommended format: "{EntityType}:{EntityId}").</param>
    /// <param name="entity">Entity instance to cache.</param>
    void Set<TEntity>(string key, TEntity entity) where TEntity : class;

    /// <summary>
    /// Retrieves an entity from the cache.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity to retrieve (must match type used in Set).</typeparam>
    /// <param name="key">Cache key used in Set operation.</param>
    /// <returns>Cached entity if found and type matches, otherwise null.</returns>
    TEntity? Get<TEntity>(string key) where TEntity : class;

    /// <summary>
    /// Attempts to retrieve an entity from the cache.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity to retrieve (must match type used in Set).</typeparam>
    /// <param name="key">Cache key used in Set operation.</param>
    /// <param name="entity">Cached entity if found, otherwise null.</param>
    /// <returns>True if entity found and type matches, otherwise false.</returns>
    bool TryGet<TEntity>(string key, out TEntity? entity) where TEntity : class;

    /// <summary>
    /// Clears all cached entities.
    /// Primarily used for testing scenarios to ensure clean state between tests.
    /// </summary>
    void Clear();
}
