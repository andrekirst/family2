namespace FamilyHub.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed User identifier.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct UserId
{
    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("UserId cannot be empty.");
        }

        return Validation.Ok;
    }

    public static UserId New() => From(Guid.NewGuid());
}
