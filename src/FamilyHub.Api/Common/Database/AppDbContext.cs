using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace FamilyHub.Api.Common.Database;

/// <summary>
/// Application database context for Family Hub
/// Uses PostgreSQL with schema separation for organization
/// Publishes domain events after successful persistence
/// </summary>
public class AppDbContext : DbContext
{
    private readonly IMessageBus? _messageBus;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, IMessageBus messageBus) : base(options)
    {
        _messageBus = messageBus;
    }

    /// <summary>
    /// Users authenticated via OAuth (Keycloak)
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Family households
    /// </summary>
    public DbSet<Family> Families { get; set; }

    /// <summary>
    /// Calendar events
    /// </summary>
    public DbSet<CalendarEvent> CalendarEvents { get; set; }

    /// <summary>
    /// Calendar event attendees (join table)
    /// </summary>
    public DbSet<CalendarEventAttendee> CalendarEventAttendees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure PostgreSQL schemas exist
        modelBuilder.HasDefaultSchema("public");

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Override SaveChanges to automatically update UpdatedAt timestamps.
    /// Note: Aggregates (User, Family) manage their own timestamps via domain methods.
    /// </summary>
    public override int SaveChanges()
    {
        UpdateNonAggregateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update UpdatedAt timestamps
    /// and publish domain events after successful persistence.
    /// Note: Aggregates (User, Family) manage their own timestamps via domain methods.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateNonAggregateTimestamps();

        // Collect domain events from all aggregate root types before saving
        var aggregateEntries = ChangeTracker.Entries()
            .Where(e => e.Entity.GetType().BaseType?.IsGenericType == true &&
                       e.Entity.GetType().BaseType.GetGenericTypeDefinition() == typeof(AggregateRoot<>))
            .ToList();

        var domainEvents = new List<IDomainEvent>();
        foreach (var entry in aggregateEntries)
        {
            var domainEventsProperty = entry.Entity.GetType().GetProperty("DomainEvents");
            if (domainEventsProperty != null)
            {
                var events = domainEventsProperty.GetValue(entry.Entity) as IEnumerable<IDomainEvent>;
                if (events != null)
                {
                    domainEvents.AddRange(events);
                }
            }
        }

        // Save changes first
        var result = await base.SaveChangesAsync(cancellationToken);

        // Publish domain events after successful save (only if message bus is available)
        if (_messageBus is not null)
        {
            foreach (var domainEvent in domainEvents)
            {
                await _messageBus.PublishAsync(domainEvent);
            }
        }

        // Clear domain events from aggregates
        foreach (var entry in aggregateEntries)
        {
            var clearMethod = entry.Entity.GetType().GetMethod("ClearDomainEvents");
            clearMethod?.Invoke(entry.Entity, null);
        }

        return result;
    }

    /// <summary>
    /// Update timestamps for non-aggregate entities.
    /// Aggregates manage their own timestamps via domain methods.
    /// </summary>
    private void UpdateNonAggregateTimestamps()
    {
        // Currently no non-aggregate entities with timestamps
        // This method reserved for future use
        // Note: User and Family (aggregates) manage their own UpdatedAt via domain methods
    }
}
