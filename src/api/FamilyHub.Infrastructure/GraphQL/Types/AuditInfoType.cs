namespace FamilyHub.Infrastructure.GraphQL.Types;

/// <summary>
/// GraphQL type for audit metadata.
/// Provides read-only access to entity timestamps managed by TimestampInterceptor.
/// </summary>
/// <remarks>
/// This type abstracts away the infrastructure concern of timestamps from the domain model,
/// providing a clean separation between domain entities and their audit trail.
/// All timestamps are in UTC.
/// </remarks>
public sealed record AuditInfoType
{
    /// <summary>
    /// When the entity was created (UTC).
    /// Set automatically by TimestampInterceptor on first save.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the entity was last updated (UTC).
    /// Set automatically by TimestampInterceptor on every save.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }
}
