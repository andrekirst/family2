using Vogen;

namespace FamilyHub.EventChain.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ChainExecutionId
{
    public static ChainExecutionId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("Chain execution ID cannot be empty");

        return Validation.Ok;
    }
}
