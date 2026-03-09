using System.Text.RegularExpressions;

namespace FamilyHub.Api.Common.Infrastructure.Validation;

/// <summary>
/// Validates regex patterns for safety before execution.
/// Prevents ReDoS (Regular Expression Denial of Service) attacks
/// by enforcing length limits and syntax validation.
/// </summary>
public static class RegexSafetyValidator
{
    public const int MaxPatternLength = 200;

    /// <summary>
    /// Validates that a regex pattern is safe to execute.
    /// Returns true if the pattern is valid and within length limits.
    /// </summary>
    public static bool IsValid(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        if (pattern.Length > MaxPatternLength)
        {
            return false;
        }

        try
        {
            // Validate syntax by attempting to create the regex with a short timeout
            _ = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(10));
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
