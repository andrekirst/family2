namespace FamilyHub.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed Family identifier.
/// Note: Validation allows Guid.Empty for EF Core materialization.
/// Domain logic should ensure non-empty GUIDs are used in business operations.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyId
{
    public static FamilyId New() => From(Guid.NewGuid());
}
