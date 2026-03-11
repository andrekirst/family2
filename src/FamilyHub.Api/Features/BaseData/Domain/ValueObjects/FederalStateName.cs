using Vogen;

namespace FamilyHub.Api.Features.BaseData.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FederalStateName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Federal state name cannot be empty");
        }

        if (value.Length > 100)
        {
            return Validation.Invalid("Federal state name must not exceed 100 characters");
        }

        return Validation.Ok;
    }
}
