using System.Text.RegularExpressions;
using Vogen;

namespace FamilyHub.EventChain.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ActionVersion
{
    private static readonly Regex ValidPattern = new(@"^v\d+(\.\d+)*$", RegexOptions.Compiled);

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Action version cannot be empty");

        if (value.Length > 20)
            return Validation.Invalid("Action version cannot exceed 20 characters");

        if (!ValidPattern.IsMatch(value))
            return Validation.Invalid("Action version must follow format: v1, v1.0, v1.0.0");

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
        => input?.Trim().ToLowerInvariant() ?? string.Empty;
}
