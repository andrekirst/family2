using System.Text.RegularExpressions;
using Vogen;

namespace FamilyHub.EventChain.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct StepAlias
{
    private static readonly Regex ValidPattern = RegexValidPattern();

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Step alias cannot be empty");
        }

        if (value.Length > 50)
        {
            return Validation.Invalid("Step alias cannot exceed 50 characters");
        }

        if (!ValidPattern.IsMatch(value))
        {
            return Validation.Invalid("Step alias must be alphanumeric with underscores, starting with a letter or underscore");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input) => input.Trim();
    [GeneratedRegex(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled)]
    private static partial Regex RegexValidPattern();
}
