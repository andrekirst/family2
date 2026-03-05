using Vogen;

namespace FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

/// <summary>
/// Display name for a conversation (e.g. "General", "Mom &amp; Dad", "Trip Planning").
/// Max 255 characters, non-empty.
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct ConversationName
{
    public const int MaxLength = 255;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Conversation name cannot be empty");
        }

        if (value.Length > MaxLength)
        {
            return Validation.Invalid($"Conversation name cannot exceed {MaxLength} characters");
        }

        return Validation.Ok;
    }
}
