namespace FamilyHub.Infrastructure.Email.Models;

/// <summary>
/// Represents an email message to be sent.
/// </summary>
public sealed record EmailMessage
{
    /// <summary>
    /// Gets the recipient email address.
    /// </summary>
    public required string To { get; init; }

    /// <summary>
    /// Gets the recipient display name.
    /// </summary>
    public required string ToName { get; init; }

    /// <summary>
    /// Gets the email subject line.
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// Gets the HTML body content of the email.
    /// </summary>
    public required string HtmlBody { get; init; }

    /// <summary>
    /// Gets the optional plain text body content of the email.
    /// </summary>
    public string? TextBody { get; init; }

    /// <summary>
    /// Gets the optional reply-to email address.
    /// </summary>
    public string? ReplyTo { get; init; }
}
