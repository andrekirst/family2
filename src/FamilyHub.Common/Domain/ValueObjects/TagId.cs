using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct TagId
{
    public static TagId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("Tag ID cannot be empty");
        return Validation.Ok;
    }
}
