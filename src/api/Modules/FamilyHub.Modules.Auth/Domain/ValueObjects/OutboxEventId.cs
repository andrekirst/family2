using Vogen;

namespace FamilyHub.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Unique identifier for an outbox event.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public readonly partial struct OutboxEventId
{
    /// <summary>
    /// Creates a new outbox event identifier with a unique GUID.
    /// </summary>
    public static OutboxEventId New() => From(Guid.NewGuid());
}
