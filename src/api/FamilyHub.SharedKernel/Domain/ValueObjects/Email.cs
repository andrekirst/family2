using System.Text.RegularExpressions;

namespace FamilyHub.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a valid email address.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Email cannot be empty.");
        }

        if (value.Length > 320) // RFC 5321
        {
            return Validation.Invalid("Email cannot exceed 320 characters.");
        }

        if (!EmailRegex.IsMatch(value))
        {
            return Validation.Invalid("Email format is invalid.");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();
}
