namespace FamilyHub.Common.Application;

/// <summary>
/// Structured domain error returned by handlers via Result&lt;T&gt;.
/// Replaces DomainException for expected business logic failures.
///
/// ErrorCode is a stable, localizable identifier (e.g., "FAMILY_ALREADY_EXISTS").
/// Category determines the HTTP/GraphQL error mapping:
///   Validation → 400, NotFound → 404, Conflict → 409, Forbidden → 403
/// </summary>
public sealed record DomainError(
    string ErrorCode,
    string Message,
    DomainErrorCategory Category)
{
    public static DomainError Validation(string errorCode, string message) =>
        new(errorCode, message, DomainErrorCategory.Validation);

    public static DomainError NotFound(string errorCode, string message) =>
        new(errorCode, message, DomainErrorCategory.NotFound);

    public static DomainError Conflict(string errorCode, string message) =>
        new(errorCode, message, DomainErrorCategory.Conflict);

    public static DomainError Forbidden(string errorCode, string message) =>
        new(errorCode, message, DomainErrorCategory.Forbidden);

    public static DomainError BusinessRule(string errorCode, string message) =>
        new(errorCode, message, DomainErrorCategory.BusinessRule);
}

/// <summary>
/// Categories of domain errors that map to GraphQL error types and HTTP status codes.
/// </summary>
public enum DomainErrorCategory
{
    /// <summary>Input validation failure (400)</summary>
    Validation,

    /// <summary>Entity not found (404)</summary>
    NotFound,

    /// <summary>State conflict, e.g., duplicate or concurrent edit (409)</summary>
    Conflict,

    /// <summary>Permission denied (403)</summary>
    Forbidden,

    /// <summary>Business rule violation (422)</summary>
    BusinessRule
}
