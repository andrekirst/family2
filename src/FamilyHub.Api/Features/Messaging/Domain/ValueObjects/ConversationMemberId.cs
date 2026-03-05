using Vogen;

namespace FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for a conversation member record.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ConversationMemberId
{
    public static ConversationMemberId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("Conversation member ID cannot be empty");
        }

        return Validation.Ok;
    }
}
