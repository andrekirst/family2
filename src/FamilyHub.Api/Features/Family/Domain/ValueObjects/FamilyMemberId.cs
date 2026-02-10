using Vogen;

namespace FamilyHub.Api.Features.Family.Domain.ValueObjects;

/// <summary>
/// Family member identifier value object.
/// Strongly-typed wrapper around Guid for family member IDs.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyMemberId
{
    public static FamilyMemberId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("Family member ID cannot be empty");
        }

        return Validation.Ok;
    }
}
