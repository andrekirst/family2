using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for a conversation.
/// Shared across modules since messages reference conversations.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ConversationId
{
    public static ConversationId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("Conversation ID cannot be empty");
        }

        return Validation.Ok;
    }
}
