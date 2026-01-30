using FamilyHub.Api.Domain.Base;
using FamilyHub.Api.Domain.Entities;
using FamilyHub.Api.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Infrastructure.Persistence;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IPublisher publisher) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect all domain events from modified aggregate roots before saving
        var domainEvents = ChangeTracker.Entries()
            .Select(entry => entry.Entity)
            .OfType<IAggregateRoot>()
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToList();

        // Save changes to database first
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful save
        // This ensures events are only published if the transaction succeeds
        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
