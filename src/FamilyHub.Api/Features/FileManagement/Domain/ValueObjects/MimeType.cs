using Vogen;

namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

/// <summary>
/// Validated MIME type string (e.g., "image/png", "application/pdf").
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct MimeType
{
    public static readonly MimeType OctetStream = From("application/octet-stream");

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("MIME type cannot be empty");
        if (value.Length > 255)
            return Validation.Invalid("MIME type too long (max 255 characters)");
        if (!value.Contains('/'))
            return Validation.Invalid("MIME type must contain a '/' separator");
        return Validation.Ok;
    }
}
