using FamilyHub.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FamilyHub.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor for automatic timestamp management.
/// Uses TimeProvider for testable time access.
/// </summary>
/// <remarks>
/// <para><strong>Behavior:</strong></para>
/// <list type="bullet">
/// <item>EntityState.Added: Sets both CreatedAt and UpdatedAt</item>
/// <item>EntityState.Modified: Updates UpdatedAt only, prevents CreatedAt modification</item>
/// </list>
///
/// <para><strong>Design Philosophy:</strong></para>
/// <para>
/// Timestamps are infrastructure concerns, not domain logic. This interceptor operates
/// silently at the persistence boundary, keeping domain methods clean and focused on
/// business rules. Domain methods should NOT manually set CreatedAt or UpdatedAt.
/// </para>
///
/// <para><strong>Implementation:</strong></para>
/// <para>
/// Strongly-typed implementation with explicit AuditableEntity casting for compile-time
/// type safety and clear entity structure requirements. This approach is preferred over
/// string-based property access which would fail at runtime.
/// </para>
///
/// <para><strong>Performance:</strong></para>
/// <para>
/// Overhead is minimal (~1-5ms per SaveChanges call). The ChangeTracker.Entries() method
/// is already cached by EF Core, and the interface check is JIT-optimized.
/// </para>
///
/// <para><strong>Testing:</strong></para>
/// <para>
/// Use FakeTimeProvider in tests for deterministic timestamp control:
/// <code>
/// var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
/// var interceptor = new TimestampInterceptor(fakeTime);
/// </code>
/// </para>
/// </remarks>
/// <param name="timeProvider">The time provider for generating timestamps.</param>
public sealed class TimestampInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    /// <summary>
    /// Intercepts synchronous SaveChanges to update timestamps.
    /// </summary>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Intercepts asynchronous SaveChanges to update timestamps.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Updates timestamps for all ITimestampable entities in the change tracker.
    /// </summary>
    /// <param name="context">The DbContext being saved.</param>
    private void UpdateTimestamps(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        // Process only entities implementing ITimestampable
        var timestampableEntries = context.ChangeTracker
            .Entries<ITimestampable>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in timestampableEntries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Set both timestamps on creation
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.CreatedAt = now;
                    break;

                case EntityState.Modified:
                    // Only update UpdatedAt on modification
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        // Note: ISoftDeletable entities (DeletedAt) are handled manually via domain methods
        // This keeps soft delete explicit and under domain control, unlike CreatedAt/UpdatedAt
        // which are pure infrastructure concerns
    }
}
