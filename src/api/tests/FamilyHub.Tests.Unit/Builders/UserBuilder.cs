using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Unit.Builders;

/// <summary>
/// Builder for creating User test entities.
/// Uses the builder pattern for fluent, readable test data creation.
/// </summary>
public sealed class UserBuilder
{
    private Email _email = Email.From("test@example.com");
    private PasswordHash _passwordHash = PasswordHash.FromHash("TestPasswordHash123!");
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
    /// Sets the password hash for the user.
    /// </summary>
    public UserBuilder WithPasswordHash(PasswordHash passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    /// <summary>
    /// Sets the password hash for the user from a string.
    /// </summary>
    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = PasswordHash.FromHash(passwordHash);
        return this;
    }

    /// <summary>
    /// Builds the User entity using the CreateWithPassword factory method.
    /// </summary>
    public User Build()
    {
        return User.CreateWithPassword(_email, _passwordHash, _familyId);
    }
}
