using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ProcessingLogEntryId
{
    public static ProcessingLogEntryId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty
            ? Validation.Invalid("ProcessingLogEntryId cannot be empty")
            : Validation.Ok;
}
