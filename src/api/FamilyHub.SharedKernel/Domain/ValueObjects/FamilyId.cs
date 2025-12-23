namespace FamilyHub.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed Family identifier.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyId
{
    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("FamilyId cannot be empty.");
        }

        return Validation.Ok;
    }

    public static FamilyId New() => From(Guid.NewGuid());
}
