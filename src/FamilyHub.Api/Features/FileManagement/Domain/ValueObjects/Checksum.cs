using Vogen;

namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

/// <summary>
/// SHA-256 checksum as a lowercase hex string (64 characters).
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Checksum
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Checksum cannot be empty");
        if (value.Length != 64)
            return Validation.Invalid("SHA-256 checksum must be 64 hex characters");
        if (!value.All(c => char.IsAsciiHexDigitLower(c) || char.IsAsciiDigit(c)))
            return Validation.Invalid("Checksum must contain only lowercase hex characters");
        return Validation.Ok;
    }
}
