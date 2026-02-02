using Vogen;

namespace FamilyHub.Api.Features.Auth.Domain.ValueObjects;

/// <summary>
/// External user identifier (from OAuth provider like Keycloak).
/// Strongly-typed wrapper ensuring external IDs are never null or empty.
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ExternalUserId
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("External user ID is required");
        }

        if (value.Length > 255)
        {
            return Validation.Invalid("External user ID too long (max 255 characters)");
        }

        return Validation.Ok;
    }
}
