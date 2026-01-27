namespace FamilyHub.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed AuthAuditLog identifier.
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct AuthAuditLogId
{
    /// <summary>
    /// Creates a new AuthAuditLogId with a newly generated GUID.
    /// </summary>
    public static AuthAuditLogId New() => From(Guid.NewGuid());
}
