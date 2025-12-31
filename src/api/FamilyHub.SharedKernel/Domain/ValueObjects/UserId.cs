namespace FamilyHub.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed User identifier.
/// Note: Validation allows Guid.Empty for EF Core materialization.
/// Domain logic should ensure non-empty GUIDs are used in business operations.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct UserId
{
    // No validation - allow any GUID including Guid.Empty for EF Core
    // Domain layer ensures valid GUIDs through factory methods (New())

    public static UserId New() => From(Guid.NewGuid());
}
