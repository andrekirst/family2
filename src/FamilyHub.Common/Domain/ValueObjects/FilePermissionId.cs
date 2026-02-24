using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FilePermissionId
{
    public static FilePermissionId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("FilePermissionId cannot be empty.");
        return Validation.Ok;
    }
}
