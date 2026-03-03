using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

/// <summary>
/// Message identifier value object.
/// Strongly-typed wrapper around Guid for message IDs.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct MessageId
{
    /// <summary>
    /// Creates a new unique message identifier.
    /// </summary>
    public static MessageId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("Message ID cannot be empty");
        }

        return Validation.Ok;
    }
}
