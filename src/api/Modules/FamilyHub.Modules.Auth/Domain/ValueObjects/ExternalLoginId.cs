namespace FamilyHub.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed ExternalLogin identifier.
/// Used for future social provider integrations (Google, Apple, Microsoft).
/// </summary>
[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct ExternalLoginId
{
    /// <summary>
    /// Creates a new ExternalLoginId with a newly generated GUID.
    /// </summary>
    public static ExternalLoginId New() => From(Guid.NewGuid());
}
