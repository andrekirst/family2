using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain;

/// <summary>
/// Represents a link between a user and an external authentication provider.
/// Prepared for future social login integration (Google, Apple, Microsoft, etc.).
/// Currently disabled but infrastructure is in place.
/// </summary>
public sealed class ExternalLogin : Entity<ExternalLoginId>
{
    /// <summary>
    /// The user this external login is linked to.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Navigation property to the User entity.
    /// </summary>
    public User? User { get; private set; }

    /// <summary>
    /// The provider name (e.g., "google", "apple", "microsoft").
    /// </summary>
    public string Provider { get; private set; } = string.Empty;

    /// <summary>
    /// The user's unique ID at the external provider.
    /// </summary>
    public string ProviderUserId { get; private set; } = string.Empty;

    /// <summary>
    /// The email address from the external provider (may differ from account email).
    /// </summary>
    public string? ProviderEmail { get; private set; }

    /// <summary>
    /// Display name from the external provider.
    /// </summary>
    public string? ProviderDisplayName { get; private set; }

    /// <summary>
    /// When this external login was linked to the account.
    /// </summary>
    public DateTime LinkedAt { get; private set; }

    /// <summary>
    /// When this external login was last used.
    /// </summary>
    public DateTime? LastUsedAt { get; private set; }

    // Private constructor for EF Core
    private ExternalLogin() : base(ExternalLoginId.New())
    {
        UserId = UserId.From(Guid.Empty);
    }

    private ExternalLogin(
        ExternalLoginId id,
        UserId userId,
        string provider,
        string providerUserId,
        string? providerEmail,
        string? providerDisplayName) : base(id)
    {
        UserId = userId;
        Provider = provider;
        ProviderUserId = providerUserId;
        ProviderEmail = providerEmail;
        ProviderDisplayName = providerDisplayName;
        LinkedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new external login link.
    /// </summary>
    public static ExternalLogin Create(
        UserId userId,
        string provider,
        string providerUserId,
        string? providerEmail = null,
        string? providerDisplayName = null)
    {
        return new ExternalLogin(
            ExternalLoginId.New(),
            userId,
            provider.ToLowerInvariant(),
            providerUserId,
            providerEmail,
            providerDisplayName);
    }

    /// <summary>
    /// Records that this external login was used.
    /// </summary>
    public void RecordUsage()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the email from the provider.
    /// </summary>
    public void UpdateProviderEmail(string? email)
    {
        ProviderEmail = email;
    }

    /// <summary>
    /// Updates the display name from the provider.
    /// </summary>
    public void UpdateProviderDisplayName(string? displayName)
    {
        ProviderDisplayName = displayName;
    }
}
