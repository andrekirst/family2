using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct SavedSearchId
{
    public static SavedSearchId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("SavedSearchId cannot be empty.");
        return Validation.Ok;
    }
}
