using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FileVersionId
{
    public static FileVersionId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty
            ? Validation.Invalid("FileVersionId cannot be empty")
            : Validation.Ok;
}
