using Vogen;

namespace FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

/// <summary>
/// Message content value object.
/// Validates that content is non-empty and within the 4000 character limit.
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct MessageContent
{
    public const int MaxLength = 4000;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Message content cannot be empty");
        }

        if (value.Length > MaxLength)
        {
            return Validation.Invalid($"Message content cannot exceed {MaxLength} characters");
        }

        return Validation.Ok;
    }
}
