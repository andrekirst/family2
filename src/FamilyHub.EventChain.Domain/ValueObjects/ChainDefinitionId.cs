using Vogen;

namespace FamilyHub.EventChain.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ChainDefinitionId
{
    public static ChainDefinitionId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty
            ? Validation.Invalid("Chain definition ID cannot be empty")
            : Validation.Ok;
}
