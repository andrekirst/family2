using FamilyHub.Modules.Auth.Application.Abstractions;

namespace FamilyHub.Modules.Auth.Application.Services;

/// <summary>
/// Scoped implementation of IValidationCache using in-memory Dictionary.
/// Thread-safe by design (scoped per HTTP request in ASP.NET Core = single-threaded).
/// </summary>
public sealed class ValidationCache : IValidationCache
{
    private readonly Dictionary<string, object> _cache = [];

    /// <inheritdoc />
    public void Set<TEntity>(string key, TEntity entity) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(entity);

        _cache[key] = entity;
    }

    /// <inheritdoc />
    public TEntity? Get<TEntity>(string key) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_cache.TryGetValue(key, out var value))
        {
            return value as TEntity;
        }

        return null;
    }

    /// <inheritdoc />
    public bool TryGet<TEntity>(string key, out TEntity? entity) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_cache.TryGetValue(key, out var value) && value is TEntity typedEntity)
        {
            entity = typedEntity;
            return true;
        }

        entity = null;
        return false;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _cache.Clear();
    }
}
