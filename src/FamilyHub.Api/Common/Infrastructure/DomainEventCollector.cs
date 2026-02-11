using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Common.Infrastructure;

/// <summary>
/// Scoped implementation of IDomainEventCollector.
/// Accumulates domain events collected by the SaveChanges interceptor
/// so the DomainEventPublishingBehavior can publish them after commit.
/// </summary>
public sealed class DomainEventCollector : IDomainEventCollector
{
    private readonly List<IDomainEvent> _events = [];

    public void AddEvents(IEnumerable<IDomainEvent> events)
    {
        _events.AddRange(events);
    }

    public IReadOnlyList<IDomainEvent> GetAndClearEvents()
    {
        var events = _events.ToList();
        _events.Clear();
        return events;
    }
}
