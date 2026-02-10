using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FamilyHub.Api.Common.Infrastructure;

/// <summary>
/// EF Core SaveChanges interceptor that collects domain events from aggregates
/// before saving. Uses IHasDomainEvents interface (no reflection).
/// Events are stored in IDomainEventCollector for later publishing by
/// the DomainEventPublishingBehavior pipeline.
/// </summary>
public sealed class DomainEventInterceptor(IDomainEventCollector collector) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            var aggregates = eventData.Context.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Count > 0)
                .ToList();

            var events = aggregates.SelectMany(a => a.DomainEvents).ToList();

            collector.AddEvents(events);

            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
