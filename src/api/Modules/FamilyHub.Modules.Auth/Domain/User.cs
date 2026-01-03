using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain;

/// <summary>
/// User aggregate root representing a registered user in the system.
/// </summary>
public class User : AggregateRoot<UserId>, ISoftDeletable
{
    private readonly List<UserFamily> _userFamilies = [];

    /// <summary>
    /// User's email address (unique identifier for login).
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Whether the email has been verified.
    /// </summary>
    public bool EmailVerified { get; private set; }

    /// <summary>
    /// When the email was verified (null if not verified).
    /// </summary>
    public DateTime? EmailVerifiedAt { get; private set; }

    /// <summary>
    /// External OAuth provider user ID (e.g., Zitadel user ID).
    /// Required field - all users authenticate via OAuth.
    /// </summary>
    public string ExternalUserId { get; private set; } = string.Empty;

    /// <summary>
    /// OAuth provider name (e.g., "zitadel").
    /// Required field - all users authenticate via OAuth.
    /// </summary>
    public string ExternalProvider { get; private set; } = string.Empty;

    /// <summary>
    /// Soft delete timestamp
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User's family memberships.
    /// </summary>
    public IReadOnlyCollection<UserFamily> UserFamilies => _userFamilies.AsReadOnly();

    // Private constructor for EF Core
    private User() : base(UserId.From(Guid.NewGuid()))
    {
        Email = Email.From("temp@temp.com"); // EF Core will set the actual value
    }

    private User(UserId id, Email email) : base(id)
    {
        Email = email;
        EmailVerified = false;
    }

    /// <summary>
    /// Creates a new user from external OAuth provider (e.g., Zitadel).
    /// This is now the ONLY way to create users.
    /// </summary>
    public static User CreateFromOAuth(Email email, string externalUserId, string externalProvider)
    {
        return new User(UserId.New(), email)
        {
            ExternalUserId = externalUserId,
            ExternalProvider = externalProvider,
            EmailVerified = true, // OAuth providers verify email
            EmailVerifiedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks the email as verified.
    /// </summary>
    public void VerifyEmail()
    {
        if (EmailVerified)
        {
            return;
        }

        EmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;

        // Domain event
        // AddDomainEvent(new EmailVerifiedEvent(Id, Email));
    }

    /// <summary>
    /// Soft deletes the user.
    /// </summary>
    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;

        // Domain event
        // AddDomainEvent(new UserDeletedEvent(Id));
    }

    /// <summary>
    /// Adds a family membership.
    /// </summary>
    internal void AddFamilyMembership(UserFamily userFamily)
    {
        _userFamilies.Add(userFamily);
    }
}
