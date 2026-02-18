using Vogen;

namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

/// <summary>
/// Validated file or folder name. Max 255 chars, no path separators or null bytes.
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FileName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("File name cannot be empty");
        if (value.Length > 255)
            return Validation.Invalid("File name too long (max 255 characters)");
        if (value.Contains('/') || value.Contains('\\'))
            return Validation.Invalid("File name cannot contain path separators");
        if (value.Contains('\0'))
            return Validation.Invalid("File name cannot contain null bytes");
        return Validation.Ok;
    }
}
