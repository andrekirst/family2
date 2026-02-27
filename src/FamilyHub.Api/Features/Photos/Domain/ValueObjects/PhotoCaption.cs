using Vogen;

namespace FamilyHub.Api.Features.Photos.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct PhotoCaption
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Caption cannot be empty.");
        }

        if (value.Length > 500)
        {
            return Validation.Invalid("Caption cannot exceed 500 characters.");
        }

        return Validation.Ok;
    }
}
