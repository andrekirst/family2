using Vogen;

namespace FamilyHub.Api.Features.School.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct SchoolName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("School name is required");
        }

        if (value.Length < 2)
        {
            return Validation.Invalid("School name must be at least 2 characters");
        }

        if (value.Length > 200)
        {
            return Validation.Invalid("School name too long (max 200 characters)");
        }

        return Validation.Ok;
    }
}
