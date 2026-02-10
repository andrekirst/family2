using Vogen;

namespace FamilyHub.Api.Features.Family.Domain.ValueObjects;

/// <summary>
/// Invitation identifier value object.
/// Strongly-typed wrapper around Guid for invitation IDs.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct InvitationId
{
    public static InvitationId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("Invitation ID cannot be empty");
        }

        return Validation.Ok;
    }
}
