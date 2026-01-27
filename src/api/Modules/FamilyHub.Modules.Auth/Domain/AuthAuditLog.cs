using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain;

/// <summary>
/// Represents an audit log entry for authentication events.
/// Used for comprehensive security monitoring and compliance.
/// </summary>
public sealed class AuthAuditLog : Entity<AuthAuditLogId>
{
    /// <summary>
    /// The user this event relates to (null for failed attempts with unknown email).
    /// </summary>
    public UserId? UserId { get; private set; }

    /// <summary>
    /// The email involved in the event (useful for failed login attempts).
    /// </summary>
    public Email? Email { get; private set; }

    /// <summary>
    /// The type of authentication event.
    /// </summary>
    public AuthAuditEventType EventType { get; private set; }

    /// <summary>
    /// When the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; private set; }

    /// <summary>
    /// IP address from which the event originated.
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// User agent string from the request.
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Additional details about the event (stored as JSON).
    /// Examples: device info, location, specific error codes.
    /// </summary>
    public string? Details { get; private set; }

    /// <summary>
    /// Whether the event was successful.
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// If the event failed, the reason for failure.
    /// </summary>
    public string? FailureReason { get; private set; }

    // Private constructor for EF Core
    private AuthAuditLog() : base(AuthAuditLogId.New())
    {
    }

    private AuthAuditLog(
        AuthAuditLogId id,
        AuthAuditEventType eventType,
        UserId? userId,
        Email? email,
        string? ipAddress,
        string? userAgent,
        bool success,
        string? failureReason,
        string? details) : base(id)
    {
        EventType = eventType;
        UserId = userId;
        Email = email;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Success = success;
        FailureReason = failureReason;
        Details = details;
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a successful audit log entry.
    /// </summary>
    public static AuthAuditLog CreateSuccess(
        AuthAuditEventType eventType,
        UserId userId,
        Email? email = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? details = null)
    {
        return new AuthAuditLog(
            AuthAuditLogId.New(),
            eventType,
            userId,
            email,
            ipAddress,
            userAgent,
            success: true,
            failureReason: null,
            details);
    }

    /// <summary>
    /// Creates a failed audit log entry.
    /// </summary>
    public static AuthAuditLog CreateFailure(
        AuthAuditEventType eventType,
        string failureReason,
        UserId? userId = null,
        Email? email = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? details = null)
    {
        return new AuthAuditLog(
            AuthAuditLogId.New(),
            eventType,
            userId,
            email,
            ipAddress,
            userAgent,
            success: false,
            failureReason,
            details);
    }

    /// <summary>
    /// Creates an audit log entry with explicit success/failure status.
    /// </summary>
    public static AuthAuditLog Create(
        UserId? userId,
        Email? email,
        AuthAuditEventType eventType,
        string? ipAddress,
        string? userAgent,
        bool success,
        string? failureReason)
    {
        return new AuthAuditLog(
            AuthAuditLogId.New(),
            eventType,
            userId,
            email,
            ipAddress,
            userAgent,
            success,
            failureReason,
            details: null);
    }
}
