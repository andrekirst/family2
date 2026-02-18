using Vogen;

namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct AlbumName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Album name cannot be empty.");
        if (value.Length > 100)
            return Validation.Invalid("Album name cannot exceed 100 characters.");
        return Validation.Ok;
    }
}
