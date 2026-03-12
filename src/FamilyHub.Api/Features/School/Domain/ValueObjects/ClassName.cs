using Vogen;

namespace FamilyHub.Api.Features.School.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ClassName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Class name is required");
        }

        if (value.Length > 20)
        {
            return Validation.Invalid("Class name too long (max 20 characters)");
        }

        return Validation.Ok;
    }
}
