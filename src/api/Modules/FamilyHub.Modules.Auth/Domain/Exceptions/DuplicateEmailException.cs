using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to register a user with an email that already exists.
/// </summary>
public sealed class DuplicateEmailException : Exception
{
    /// <summary>
    /// Gets the email address that caused the duplicate conflict.
    /// </summary>
    public Email Email { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateEmailException"/> class.
    /// </summary>
    /// <param name="email">The email address that already exists.</param>
    public DuplicateEmailException(Email email)
        : base($"A user with email '{email.Value}' already exists.")
    {
        Email = email;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateEmailException"/> class with an inner exception.
    /// </summary>
    /// <param name="email">The email address that already exists.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public DuplicateEmailException(Email email, Exception innerException)
        : base($"A user with email '{email.Value}' already exists.", innerException)
    {
        Email = email;
    }
}
