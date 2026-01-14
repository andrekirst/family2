namespace FamilyHub.SharedKernel.ValueObjects;

using Vogen;

/// <summary>
/// Unique identifier for an EmailOutbox entity.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct EmailOutboxId
{
    /// <summary>
    /// Creates a new EmailOutboxId with a unique GUID value.
    /// </summary>
    public static EmailOutboxId New() => From(Guid.NewGuid());
}
