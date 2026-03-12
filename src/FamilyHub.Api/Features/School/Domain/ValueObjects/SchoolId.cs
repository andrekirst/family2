using Vogen;

namespace FamilyHub.Api.Features.School.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct SchoolId
{
    public static SchoolId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("School ID cannot be empty");
        }

        return Validation.Ok;
    }
}
