namespace FamilyHub.Api.Common.Infrastructure.Audit;

/// <summary>
/// Optional marker interface for domain events that relate to a specific entity.
/// When implemented, the AuditEventPersister uses these values to populate
/// entity_type and entity_id in the audit record for efficient querying.
///
/// Domain events that don't implement this interface will have null entity columns.
/// </summary>
public interface IEntityDomainEvent
{
    /// <summary>
    /// The type of entity this event relates to (e.g. "Family", "User").
    /// </summary>
    string EntityType { get; }

    /// <summary>
    /// The string representation of the entity ID.
    /// </summary>
    string EntityId { get; }
}
