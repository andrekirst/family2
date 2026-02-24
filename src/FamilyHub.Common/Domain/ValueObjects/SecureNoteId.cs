using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct SecureNoteId
{
    public static SecureNoteId New() => From(Guid.NewGuid());
}
