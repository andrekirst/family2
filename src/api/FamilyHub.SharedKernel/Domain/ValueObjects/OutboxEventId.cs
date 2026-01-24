
using Vogen;

namespace FamilyHub.SharedKernel.ValueObjects;
/// <summary>
/// Unique identifier for an OutboxEvent entity.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct OutboxEventId
{
    /// <summary>
    /// Creates a new OutboxEventId with a unique GUID value.
    /// </summary>
    public static OutboxEventId New() => From(Guid.NewGuid());
}
