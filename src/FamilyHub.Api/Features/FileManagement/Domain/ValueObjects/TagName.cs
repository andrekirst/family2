using Vogen;

namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct TagName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Tag name cannot be empty");
        if (value.Length > 50)
            return Validation.Invalid("Tag name too long (max 50 characters)");
        return Validation.Ok;
    }
}
