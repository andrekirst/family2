using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

/// <summary>
/// Folder identifier value object.
/// Strongly-typed wrapper around Guid for folder IDs.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FolderId
{
    public static FolderId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("Folder ID cannot be empty");
        return Validation.Ok;
    }
}
