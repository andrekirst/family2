using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

/// <summary>
/// Avatar identifier value object.
/// Strongly-typed wrapper around Guid for avatar IDs.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct AvatarId
{
    public static AvatarId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("Avatar ID cannot be empty");
        }

        return Validation.Ok;
    }
}
