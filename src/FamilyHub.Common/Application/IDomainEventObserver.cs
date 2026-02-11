namespace FamilyHub.Common.Application;

/// <summary>
/// Observer for domain events published after transaction commit.
/// Implementations receive ALL domain events regardless of type,
/// enabling cross-cutting concerns like event chain triggers.
/// </summary>
public interface IDomainEventObserver
{
    Task OnEventPublishedAsync(Domain.IDomainEvent @event, CancellationToken ct = default);
}
