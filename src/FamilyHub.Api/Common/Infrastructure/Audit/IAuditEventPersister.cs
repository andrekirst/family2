using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Common.Infrastructure.Audit;

/// <summary>
/// Persists domain events as immutable audit records.
/// </summary>
public interface IAuditEventPersister
{
    /// <summary>
    /// Serialize and persist a domain event to the audit_events table.
    /// </summary>
    Task PersistAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
