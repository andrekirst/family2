using FamilyHub.Common.Domain;

namespace FamilyHub.Common.Application;

/// <summary>
/// Scoped service that bridges the SaveChanges interceptor and the
/// DomainEventPublishingBehavior pipeline. The interceptor collects
/// events here; the behavior reads and clears them after commit.
/// </summary>
public interface IDomainEventCollector
{
    void AddEvents(IEnumerable<IDomainEvent> events);
    IReadOnlyList<IDomainEvent> GetAndClearEvents();
}
