namespace FamilyHub.SharedKernel.Domain;

/// <summary>
/// Base class for all entities in the domain.
/// Entities are objects that have a unique identity that runs through time and different states.
/// All entities automatically track CreatedAt and UpdatedAt timestamps via ITimestampable.
/// </summary>
/// <remarks>
/// <para><strong>Timestamp Management:</strong></para>
/// <para>
/// Timestamps are managed automatically by the TimestampInterceptor. Domain methods should NOT
/// manually set CreatedAt or UpdatedAt - the interceptor handles this at the persistence boundary.
/// </para>
/// <para><strong>Design Decision:</strong></para>
/// <para>
/// All entities get timestamps for consistency and simplicity. This eliminates the need for a
/// separate AuditableEntity layer. If an entity doesn't need timestamps for business purposes,
/// they simply won't be queried, but they'll still be tracked for infrastructure/audit reasons.
/// </para>
/// </remarks>
public abstract class Entity<TId>(TId id) : IEquatable<Entity<TId>>, ITimestampable
    where TId : notnull
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    public TId Id { get; } = id;

    /// <summary>
    /// When the entity was created. Set automatically on first save.
    /// </summary>
    /// <remarks>
    /// Public setter required for EF Core interceptor access.
    /// Protection is achieved through protected constructors and domain method discipline.
    /// </remarks>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the entity was last updated. Set automatically on every save.
    /// </summary>
    /// <remarks>
    /// Public setter required for ITimestampable interface implementation.
    /// Protection is via protected constructors and domain method discipline.
    /// </remarks>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Parameterless constructor required for EF Core entity materialization.
    /// </summary>
    protected Entity() : this(default!)
    {
        // Required for EF Core
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>true if the specified object is equal to the current entity; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Entity<TId>)obj);
    }

    /// <summary>
    /// Determines whether the specified entity is equal to the current entity by comparing identifiers.
    /// </summary>
    /// <param name="other">The entity to compare with the current entity.</param>
    /// <returns>true if the specified entity has the same identifier; otherwise, false.</returns>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Returns the hash code for this entity based on its identifier.
    /// </summary>
    /// <returns>A hash code for the current entity.</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Determines whether two entities are equal by comparing their identifiers.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns>true if the entities have the same identifier; otherwise, false.</returns>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two entities are not equal by comparing their identifiers.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns>true if the entities have different identifiers; otherwise, false.</returns>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
