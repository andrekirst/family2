using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

/// <summary>
/// File identifier value object.
/// Strongly-typed wrapper around Guid for file storage IDs.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FileId
{
    public static FileId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("File ID cannot be empty");
        }

        return Validation.Ok;
    }
}
