using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Unit.Builders;

/// <summary>
/// Builder for creating User test entities.
/// Uses the builder pattern for fluent, readable test data creation.
/// </summary>
public sealed class UserBuilder
{
    private Email _email = Email.From("test@example.com");
    private string _externalUserId = $"zitadel_{Guid.NewGuid():N}";
    private string _externalProvider = "zitadel";
    private FamilyId _familyId = FamilyId.New();

    /// <summary>
    /// Sets the email for the user.
    /// </summary>
    public UserBuilder WithEmail(Email email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the email for the user from a string.
    /// </summary>
    public UserBuilder WithEmail(string email)
    {
        _email = Email.From(email);
        return this;
    }

    /// <summary>
    /// Sets the FamilyId for the user.
    /// </summary>
    public UserBuilder WithFamilyId(FamilyId familyId)
    {
        _familyId = familyId;
        return this;
    }

    /// <summary>
    /// Sets the external user ID (from OAuth provider).
    /// </summary>
    public UserBuilder WithExternalUserId(string externalUserId)
    {
        _externalUserId = externalUserId;
        return this;
    }

    /// <summary>
    /// Sets the external provider name.
    /// </summary>
    public UserBuilder WithExternalProvider(string externalProvider)
    {
        _externalProvider = externalProvider;
        return this;
    }

    /// <summary>
    /// Builds the User entity using the CreateFromOAuth factory method.
    /// </summary>
    public User Build()
    {
        return User.CreateFromOAuth(_email, _externalUserId, _externalProvider, _familyId);
    }
}
