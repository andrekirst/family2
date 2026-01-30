using System.Text.RegularExpressions;
using Vogen;

namespace FamilyHub.Api.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email cannot be empty.");

        if (value.Length > 320)
            return Validation.Invalid("Email cannot exceed 320 characters.");

        if (!EmailRegex.IsMatch(value))
            return Validation.Invalid("Invalid email format.");

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
        => input?.Trim().ToLowerInvariant() ?? string.Empty;
}
