using Vogen;

namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

/// <summary>
/// File size in bytes. Must be non-negative.
/// </summary>
[ValueObject<long>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FileSize
{
    private static Validation Validate(long value)
    {
        if (value < 0)
            return Validation.Invalid("File size cannot be negative");
        return Validation.Ok;
    }

    public string ToHumanReadable()
    {
        const long kb = 1024;
        const long mb = kb * 1024;
        const long gb = mb * 1024;

        return Value switch
        {
            >= gb => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F1} GB", Value / (double)gb),
            >= mb => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F1} MB", Value / (double)mb),
            >= kb => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F1} KB", Value / (double)kb),
            _ => $"{Value} B"
        };
    }
}
