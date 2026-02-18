using System.Text.RegularExpressions;
using Vogen;

namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct TagColor
{
    private static readonly Regex HexColorPattern = new(@"^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Tag color cannot be empty");
        if (!HexColorPattern.IsMatch(value))
            return Validation.Invalid("Tag color must be a valid hex color (e.g., #FF5733)");
        return Validation.Ok;
    }
}
