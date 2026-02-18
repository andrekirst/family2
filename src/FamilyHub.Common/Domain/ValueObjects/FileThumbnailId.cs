using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FileThumbnailId
{
    public static FileThumbnailId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("FileThumbnailId cannot be empty") : Validation.Ok;
}
