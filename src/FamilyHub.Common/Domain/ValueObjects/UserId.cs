using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

/// <summary>
/// User identifier value object.
/// Strongly-typed wrapper around Guid for user IDs.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct UserId
{
    /// <summary>
    /// Creates a new unique user identifier.
    /// </summary>
    public static UserId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("User ID cannot be empty");
        }

        return Validation.Ok;
    }
}
