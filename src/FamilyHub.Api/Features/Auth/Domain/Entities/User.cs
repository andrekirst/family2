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
    /// User's global avatar ID (null if no avatar uploaded yet)
    /// </summary>
    public AvatarId? AvatarId { get; private set; }

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
        DateTimeOffset utcNow,
        string? username = null)
    {
        var now = utcNow;
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
            CreatedAt = now.UtcDateTime,
            LastLoginAt = now.UtcDateTime,
            UpdatedAt = now.UtcDateTime
        };

        user.RaiseDomainEvent(new UserRegisteredEvent(
            user.Id,
            user.Email,
            user.Name,
            user.ExternalUserId,
            user.EmailVerified,
            now.UtcDateTime
        ));

        return user;
    }

    /// <summary>
    /// Update the user's last login timestamp.
    /// Called when user authenticates via OAuth.
    /// </summary>
    public void UpdateLastLogin(DateTime loginTime, DateTimeOffset utcNow)
    {
        var now = utcNow;
        LastLoginAt = loginTime;
        UpdatedAt = now.UtcDateTime;
    }

    /// <summary>
    /// Re-link the user to a new external identity provider ID.
    /// Used when the OAuth provider (e.g. Keycloak realm) is recreated.
    /// </summary>
    public void UpdateExternalId(ExternalUserId externalUserId, DateTimeOffset utcNow)
    {
        var now = utcNow;
        ExternalUserId = externalUserId;
        UpdatedAt = now.UtcDateTime;
    }

    /// <summary>
    /// Update user's basic profile information from OAuth provider.
    /// </summary>
    public void UpdateProfile(Email email, UserName name, bool emailVerified, DateTimeOffset utcNow)
    {
        var now = utcNow;
        Email = email;
        Name = name;
        EmailVerified = emailVerified;
        UpdatedAt = now.UtcDateTime;
    }

    /// <summary>
    /// Assign user to a family.
    /// Raises UserFamilyAssignedEvent.
    /// </summary>
    /// <exception cref="DomainException">If user is already assigned to a family</exception>
    public void AssignToFamily(FamilyId familyId, DateTimeOffset utcNow)
    {
        if (FamilyId is not null)
        {
            throw new DomainException("User is already assigned to a family", DomainErrorCodes.UserAlreadyAssignedToFamily);
        }

        var now = utcNow;
        FamilyId = familyId;
        UpdatedAt = now.UtcDateTime;

        RaiseDomainEvent(new UserFamilyAssignedEvent(
            Id,
            familyId,
            now.UtcDateTime
        ));
    }

    /// <summary>
    /// Remove user from their current family.
    /// </summary>
    /// <exception cref="DomainException">If user is not assigned to any family</exception>
    public void RemoveFromFamily(DateTimeOffset utcNow)
    {
        if (FamilyId is null)
        {
            throw new DomainException("User is not assigned to any family", DomainErrorCodes.UserNotAssignedToFamily);
        }

        var now = utcNow;
        var previousFamilyId = FamilyId.Value;
        FamilyId = null;
        UpdatedAt = now.UtcDateTime;

        RaiseDomainEvent(new UserFamilyRemovedEvent(
            Id,
            previousFamilyId,
            now.UtcDateTime
        ));
    }

    /// <summary>
    /// Update user's preferred locale for UI language.
    /// </summary>
    public void UpdateLocale(string locale, DateTimeOffset utcNow)
    {
        var now = utcNow;
        PreferredLocale = locale;
        UpdatedAt = now.UtcDateTime;
    }

    /// <summary>
    /// Set or update the user's global avatar.
    /// Raises UserAvatarChangedEvent.
    /// </summary>
    public void SetAvatar(AvatarId avatarId, DateTimeOffset utcNow)
    {
        var now = utcNow;
        var previousAvatarId = AvatarId;
        AvatarId = avatarId;
        UpdatedAt = now.UtcDateTime;

        RaiseDomainEvent(new UserAvatarChangedEvent(
            Id,
            avatarId,
            previousAvatarId,
            now.UtcDateTime
        ));
    }

    /// <summary>
    /// Remove the user's global avatar.
    /// Raises UserAvatarRemovedEvent.
    /// </summary>
    public void RemoveAvatar(DateTimeOffset utcNow)
    {
        if (AvatarId is null)
        {
            return;
        }

        var now = utcNow;
        var previousAvatarId = AvatarId.Value;
        AvatarId = null;
        UpdatedAt = now.UtcDateTime;

        RaiseDomainEvent(new UserAvatarRemovedEvent(
            Id,
            previousAvatarId,
            now.UtcDateTime
        ));
    }

    /// <summary>
    /// Deactivate the user account.
    /// </summary>
    public void Deactivate(DateTimeOffset utcNow)
    {
        var now = utcNow;
        IsActive = false;
        UpdatedAt = now.UtcDateTime;
    }

    /// <summary>
    /// Reactivate the user account.
    /// </summary>
    public void Reactivate(DateTimeOffset utcNow)
    {
        var now = utcNow;
        IsActive = true;
        UpdatedAt = now.UtcDateTime;
    }
}
