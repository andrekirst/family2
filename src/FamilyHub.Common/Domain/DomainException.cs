namespace FamilyHub.Common.Domain;

/// <summary>
/// Exception thrown when a domain rule is violated.
/// Represents business logic violations rather than technical errors.
/// ErrorCode provides a stable identifier for localization — the GraphQL error filter
/// maps error codes to localized messages via IStringLocalizer.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Stable error code used as a localization key (e.g. "INVITATION_EXPIRED").
    /// Null when no specific error code applies (falls back to exception message).
    /// </summary>
    public string? ErrorCode { get; }

    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
