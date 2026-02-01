namespace FamilyHub.Api.Features.Auth.Models;

/// <summary>
/// User entity representing a family hub user authenticated via OAuth (Keycloak)
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address (unique, required)
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's display name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Username for child accounts (optional)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// External OAuth provider user ID (Keycloak user ID)
    /// </summary>
    public required string ExternalUserId { get; set; }

    /// <summary>
    /// OAuth provider name (currently only KEYCLOAK supported)
    /// </summary>
    public required string ExternalProvider { get; set; }

    /// <summary>
    /// Associated family ID (null if user hasn't created/joined a family yet)
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// Whether the user's email has been verified by the OAuth provider
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// When the user record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to Family
    /// </summary>
    public Family.Models.Family? Family { get; set; }
}
