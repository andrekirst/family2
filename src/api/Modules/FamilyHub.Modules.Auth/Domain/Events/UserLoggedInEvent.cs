using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Events;

/// <summary>
/// Domain event raised when a user completes OAuth login.
/// Used to trigger profile synchronization with Zitadel.
/// </summary>
public sealed class UserLoggedInEvent : DomainEvent
{
    /// <summary>
    /// The internal Family Hub user ID.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// The Zitadel user ID (sub claim).
    /// </summary>
    public string ExternalUserId { get; }

    /// <summary>
    /// The identity provider name (e.g., "zitadel").
    /// </summary>
    public string ExternalProvider { get; }

    /// <summary>
    /// The display name from the identity provider's ID token.
    /// </summary>
    public string? DisplayNameFromProvider { get; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public Email Email { get; }

    /// <summary>
    /// Indicates whether this is a newly created user.
    /// </summary>
    public bool IsNewUser { get; }

    /// <summary>
    /// Creates a new UserLoggedInEvent.
    /// </summary>
    public UserLoggedInEvent(
        UserId userId,
        string externalUserId,
        string externalProvider,
        string? displayNameFromProvider,
        Email email,
        bool isNewUser)
    {
        UserId = userId;
        ExternalUserId = externalUserId;
        ExternalProvider = externalProvider;
        DisplayNameFromProvider = displayNameFromProvider;
        Email = email;
        IsNewUser = isNewUser;
    }
}
