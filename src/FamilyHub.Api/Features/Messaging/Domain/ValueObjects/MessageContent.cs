using Vogen;

namespace FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

/// <summary>
/// Message content value object.
/// Empty string is allowed (attachment-only messages). Validates max length.
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct MessageContent
{
    public const int MaxLength = 4000;

    public static readonly MessageContent Empty = From(string.Empty);

    private static Validation Validate(string value)
    {
        if (value.Length > MaxLength)
        {
            return Validation.Invalid($"Message content cannot exceed {MaxLength} characters");
        }

        return Validation.Ok;
    }
}
