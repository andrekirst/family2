using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Events;

namespace FamilyHub.Api.Features.Auth.Domain.Entities;

/// <summary>
/// User aggregate root representing a family hub user authenticated via OAuth (Keycloak).
/// Encapsulates user identity, OAuth integration, and family membership logic.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    // Private parameterless constructor for EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private User() { }
#pragma warning restore CS8618

    /// <summary>
    /// User's email address (unique, required)
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// User's display name
    /// </summary>
    public UserName Name { get; private set; }

    /// <summary>
    /// Username for child accounts (optional)
    /// </summary>
    public string? Username { get; private set; }

    /// <summary>
    /// External OAuth provider user ID (Keycloak user ID)
    /// </summary>
    public ExternalUserId ExternalUserId { get; private set; }

    /// <summary>
    /// OAuth provider name (currently only KEYCLOAK supported)
    /// </summary>
    public string ExternalProvider { get; private set; } = "KEYCLOAK";

    /// <summary>
    /// Associated family ID (null if user hasn't created/joined a family yet)
    /// </summary>
    public FamilyId? FamilyId { get; private set; }

    /// <summary>
    /// Whether the user's email has been verified by the OAuth provider
    /// </summary>
    public bool EmailVerified { get; private set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// When the user was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// When the user record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// User's preferred locale for UI language (e.g. "en", "de").
    /// Stored in DB for cross-device sync; also cached in localStorage for instant access.
    /// </summary>
    public string PreferredLocale { get; private set; } = "en";

    /// <summary>
    /// Navigation property to Family
    /// </summary>
    public FamilyHub.Api.Features.Family.Domain.Entities.Family? Family { get; private set; }

    /// <summary>
    /// Factory method to register a new user from OAuth provider.
    /// Raises UserRegisteredEvent.
    /// </summary>
    public static User Register(
        Email email,
        UserName name,
        ExternalUserId externalUserId,
        bool emailVerified,
        string? username = null)
    {
        var user = new User
        {
            Id = UserId.New(),
            Email = email,
            Name = name,
            ExternalUserId = externalUserId,
            EmailVerified = emailVerified,
            Username = username,
            ExternalProvider = "KEYCLOAK",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.RaiseDomainEvent(new UserRegisteredEvent(
            user.Id,
            user.Email,
            user.Name,
            user.ExternalUserId,
            user.EmailVerified,
            DateTime.UtcNow
        ));

        return user;
    }

    /// <summary>
    /// Update the user's last login timestamp.
    /// Called when user authenticates via OAuth.
    /// </summary>
    public void UpdateLastLogin(DateTime loginTime)
    {
        LastLoginAt = loginTime;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update user's basic profile information from OAuth provider.
    /// </summary>
    public void UpdateProfile(Email email, UserName name, bool emailVerified)
    {
        Email = email;
        Name = name;
        EmailVerified = emailVerified;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assign user to a family.
    /// Raises UserFamilyAssignedEvent.
    /// </summary>
    /// <exception cref="DomainException">If user is already assigned to a family</exception>
    public void AssignToFamily(FamilyId familyId)
    {
        if (FamilyId is not null)
        {
            throw new DomainException("User is already assigned to a family", DomainErrorCodes.UserAlreadyAssignedToFamily);
        }

        FamilyId = familyId;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserFamilyAssignedEvent(
            Id,
            familyId,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Remove user from their current family.
    /// </summary>
    /// <exception cref="DomainException">If user is not assigned to any family</exception>
    public void RemoveFromFamily()
    {
        if (FamilyId is null)
        {
            throw new DomainException("User is not assigned to any family", DomainErrorCodes.UserNotAssignedToFamily);
        }

        var previousFamilyId = FamilyId.Value;
        FamilyId = null;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserFamilyRemovedEvent(
            Id,
            previousFamilyId,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Update user's preferred locale for UI language.
    /// </summary>
    public void UpdateLocale(string locale)
    {
        PreferredLocale = locale;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate the user account.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivate the user account.
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
