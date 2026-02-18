using Vogen;

namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

/// <summary>
/// Opaque key identifying a file in the storage backend.
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct StorageKey
{
    public static StorageKey New() => From(Guid.NewGuid().ToString());

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Storage key cannot be empty");
        if (value.Length > 255)
            return Validation.Invalid("Storage key too long (max 255 characters)");
        return Validation.Ok;
    }
}
