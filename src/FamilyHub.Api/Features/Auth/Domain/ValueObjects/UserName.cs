using Vogen;

namespace FamilyHub.Api.Features.Auth.Domain.ValueObjects;

/// <summary>
/// User display name value object with validation.
/// Ensures user names meet minimum requirements.
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct UserName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("User name is required");
        }

        if (value.Length < 2)
        {
            return Validation.Invalid("User name must be at least 2 characters");
        }

        if (value.Length > 200)
        {
            return Validation.Invalid("User name too long (max 200 characters)");
        }

        return Validation.Ok;
    }
}
