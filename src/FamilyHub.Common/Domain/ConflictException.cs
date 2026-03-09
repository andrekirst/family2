namespace FamilyHub.Common.Domain;

/// <summary>
/// Exception thrown when an optimistic concurrency conflict is detected.
/// This occurs when two requests modify the same aggregate simultaneously —
/// the second save detects that the row version has changed since it was read.
///
/// Maps to HTTP 409 Conflict and GraphQL CONFLICT error code.
/// </summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string entityType)
        : base($"The {entityType} was modified by another request. Please reload and try again.",
              "CONCURRENCY_CONFLICT")
    {
        EntityType = entityType;
    }

    public string EntityType { get; }
}
