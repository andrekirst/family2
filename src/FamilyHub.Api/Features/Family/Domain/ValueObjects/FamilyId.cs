using Vogen;

namespace FamilyHub.Api.Features.Family.Domain.ValueObjects;

/// <summary>
/// Family identifier value object.
/// Strongly-typed wrapper around Guid for family IDs.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyId
{
    /// <summary>
    /// Creates a new unique family identifier.
    /// </summary>
    public static FamilyId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("Family ID cannot be empty");

        return Validation.Ok;
    }
}
