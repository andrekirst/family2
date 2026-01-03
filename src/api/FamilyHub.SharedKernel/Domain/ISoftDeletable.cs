namespace FamilyHub.SharedKernel.Domain;

/// <summary>
/// Marker interface for entities supporting soft deletion.
/// Soft delete is opt-in behavior - not all entities need this capability.
/// </summary>
/// <remarks>
/// Soft deletion allows entities to be marked as deleted without physical removal
/// from the database. This is useful for audit trails, data recovery, and maintaining
/// referential integrity.
///
/// Usage with EF Core:
/// Configure a global query filter to automatically exclude soft-deleted entities:
/// <code>
/// builder.HasQueryFilter(e => e.DeletedAt == null);
/// </code>
///
/// Design decision: DeletedAt is set manually via domain methods (e.g., Delete())
/// rather than automatically via interceptor. This keeps soft delete explicit and
/// under domain control, unlike CreatedAt/UpdatedAt which are infrastructure concerns.
/// </remarks>
public interface ISoftDeletable
{
    /// <summary>
    /// When the entity was soft deleted. Null if not deleted.
    /// Set manually via domain methods (e.g., Delete()).
    /// </summary>
    DateTime? DeletedAt { get; set; }
}
