using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Family.Persistence;

/// <summary>
/// Database context for the Family module.
///
/// PHASE 5 STATE: This DbContext owns Family module entities:
/// - Family aggregate: Core family entity
/// - FamilyMemberInvitation aggregate: Invitation management
///
/// SCHEMA: All tables reside in the "family" PostgreSQL schema.
///
/// CROSS-SCHEMA REFERENCES:
/// - Family.OwnerId references auth.users.id (no FK constraint, ID-only)
/// - FamilyMemberInvitation.InvitedByUserId references auth.users.id (no FK constraint, ID-only)
/// - Cross-module queries handled via IUserLookupService abstraction
///
/// ARCHITECTURE NOTES:
/// - One DbContext per module enforces bounded context boundaries
/// - Uses pooled DbContext factory for performance optimization
/// - Applies snake_case naming convention for PostgreSQL compatibility
/// </summary>
public class FamilyDbContext : DbContext
{
    private readonly IMediator? _mediator;
    private readonly ILogger<FamilyDbContext>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FamilyDbContext"/> class.
    /// Used by migrations and scenarios where domain event dispatching is not needed.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public FamilyDbContext(DbContextOptions<FamilyDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FamilyDbContext"/> class with domain event dispatching.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <param name="mediator">The MediatR instance for publishing domain events.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public FamilyDbContext(DbContextOptions<FamilyDbContext> options, IMediator mediator, ILogger<FamilyDbContext> logger) : base(options)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Family aggregates.
    /// </summary>
    public DbSet<Domain.Aggregates.Family> Families => Set<Domain.Aggregates.Family>();

    /// <summary>
    /// Family member invitations.
    /// </summary>
    public DbSet<FamilyMemberInvitation> FamilyMemberInvitations => Set<FamilyMemberInvitation>();

    /// <summary>
    /// Email outbox for tracking email delivery.
    /// </summary>
    public DbSet<Domain.EmailOutbox> EmailOutbox => Set<Domain.EmailOutbox>();

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("💾 [FamilyDbContext] SaveChangesAsync called");

        // Dispatch domain events before saving changes
        await DispatchDomainEventsAsync(cancellationToken);

        _logger?.LogInformation("💾 [FamilyDbContext] About to call base.SaveChangesAsync");

        // Save changes to database
        var result = await base.SaveChangesAsync(cancellationToken);

        _logger?.LogInformation("💾 [FamilyDbContext] SaveChangesAsync completed with {Result} changes", result);

        return result;
    }

    /// <summary>
    /// Dispatches domain events from aggregates to MediatR handlers.
    /// </summary>
    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("🔍 [FamilyDbContext] DispatchDomainEventsAsync called");

        if (_mediator == null)
        {
            _logger?.LogWarning("⚠️ [FamilyDbContext] No mediator available - skipping domain event dispatching");
            return; // No mediator available (e.g., in migrations)
        }

        // Get all tracked entities
        var entities = ChangeTracker.Entries()
            .Select(e => e.Entity)
            .ToList();

        _logger?.LogInformation("🔍 [FamilyDbContext] Found {EntityCount} tracked entities", entities.Count);

        // Collect all domain events from entities that have them
        var domainEvents = new List<DomainEvent>();
        foreach (var entity in entities)
        {
            _logger?.LogInformation("🔍 [FamilyDbContext] Checking entity type: {EntityType}", entity.GetType().Name);

            // Check if entity has DomainEvents property (duck typing approach)
            var domainEventsProperty = entity.GetType().GetProperty("DomainEvents");
            if (domainEventsProperty != null)
            {
                _logger?.LogInformation("✅ [FamilyDbContext] Entity {EntityType} HAS DomainEvents property", entity.GetType().Name);
                var events = domainEventsProperty.GetValue(entity) as IReadOnlyCollection<DomainEvent>;
                _logger?.LogInformation("🔍 [FamilyDbContext] DomainEvents value is {IsNull}, count: {Count}",
                    events == null ? "NULL" : "NOT NULL",
                    events?.Count ?? 0);

                if (events != null && events.Any())
                {
                    _logger?.LogInformation("✅ [FamilyDbContext] Entity {EntityType} has {EventCount} domain events",
                        entity.GetType().Name, events.Count);

                    domainEvents.AddRange(events);

                    // Clear domain events using ClearDomainEvents method
                    var clearMethod = entity.GetType().GetMethod("ClearDomainEvents");
                    clearMethod?.Invoke(entity, null);
                }
                else
                {
                    _logger?.LogWarning("⚠️ [FamilyDbContext] Entity {EntityType} has DomainEvents property but collection is empty or null", entity.GetType().Name);
                }
            }
            else
            {
                _logger?.LogWarning("⚠️ [FamilyDbContext] Entity {EntityType} does NOT have DomainEvents property", entity.GetType().Name);
            }
        }

        _logger?.LogInformation("📤 [FamilyDbContext] Publishing {DomainEventCount} domain events", domainEvents.Count);

        // Publish each domain event
        foreach (var domainEvent in domainEvents)
        {
            _logger?.LogInformation("📤 [FamilyDbContext] Publishing domain event: {EventType}", domainEvent.GetType().Name);
            await _mediator.Publish(domainEvent, cancellationToken);
            _logger?.LogInformation("✅ [FamilyDbContext] Successfully published {EventType}", domainEvent.GetType().Name);
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set PostgreSQL schema for this module
        modelBuilder.HasDefaultSchema("family");

        // Apply all configurations from this assembly (auto-discovery)
        // Discovers: FamilyConfiguration, FamilyMemberInvitationConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FamilyDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
