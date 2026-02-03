using Vogen;

namespace FamilyHub.Api.Features.Family.Domain.ValueObjects;

/// <summary>
/// Family name value object with validation.
/// Ensures family names meet minimum requirements.
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Family name is required");
        }

        if (value.Length < 2)
        {
            return Validation.Invalid("Family name must be at least 2 characters");
        }

        if (value.Length > 100)
        {
            return Validation.Invalid("Family name too long (max 100 characters)");
        }

        return Validation.Ok;
    }
}
