using System.Text.RegularExpressions;
using Vogen;

namespace FamilyHub.Api.Features.BaseData.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Iso3166Code
{
    private static readonly Regex Pattern = new(@"^[A-Z]{2}-[A-Z]{1,3}$", RegexOptions.Compiled);

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("ISO 3166 code cannot be empty");
        }

        if (!Pattern.IsMatch(value))
        {
            return Validation.Invalid("ISO 3166 code must match pattern XX-X to XX-XXX (e.g., DE-SN)");
        }

        return Validation.Ok;
    }
}
