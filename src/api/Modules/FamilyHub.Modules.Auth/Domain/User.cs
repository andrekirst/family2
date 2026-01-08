using FamilyDomain = FamilyHub.Modules.Family.Domain;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain;

/// <summary>
/// User aggregate root representing a registered user in the system.
/// </summary>
public class User : AggregateRoot<UserId>, ISoftDeletable
{
    /// <summary>
    /// User's email address (unique identifier for login).
    /// All users authenticate via email-based OAuth.
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
    /// The family this user belongs to.
    /// </summary>
    public FamilyId FamilyId { get; private set; }

    /// <summary>
    /// User's role in the family.
    /// </summary>
    public FamilyRole Role { get; private set; }

    /// <summary>
    /// Soft delete timestamp
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // Private constructor for EF Core
    private User() : base(UserId.From(Guid.NewGuid()))
    {
        Email = Email.From("temp@temp.com"); // EF Core will set the actual value
        FamilyId = FamilyId.From(Guid.Empty); // EF Core will set the actual value
        Role = FamilyRole.Member; // Default role
    }

    private User(UserId id, Email email, FamilyId familyId, FamilyRole? role = null) : base(id)
    {
        Email = email;
        FamilyId = familyId;
        Role = role ?? FamilyRole.Member;
        EmailVerified = false;
    }

    /// <summary>
    /// Creates a new user from external OAuth provider (e.g., Zitadel).
    /// This is now the ONLY way to create users.
    /// </summary>
    public static User CreateFromOAuth(Email email, string externalUserId, string externalProvider, FamilyId familyId)
    {
        return new User(UserId.New(), email, familyId, FamilyRole.Owner)
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
    /// Updates the user's family association.
    /// Allows switching families (e.g., when creating a new family to replace auto-created one).
    /// </summary>
    public void UpdateFamily(FamilyId newFamilyId)
    {
        FamilyId = newFamilyId;
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
    /// Gets the user's role in the given family.
    /// Returns Owner if the user owns the family, otherwise Member.
    /// </summary>
    public FamilyRole GetRoleInFamily(FamilyDomain.Family family)
    {
        return family.OwnerId == Id ? FamilyRole.Owner : FamilyRole.Member;
    }

    /// <summary>
    /// Updates the user's role in their family.
    /// Used when accepting invitations or role changes.
    /// </summary>
    public void UpdateRole(FamilyRole newRole)
    {
        Role = newRole;
    }
}
