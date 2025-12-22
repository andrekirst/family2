using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to register a user with an email that already exists.
/// </summary>
public sealed class DuplicateEmailException : Exception
{
    public Email Email { get; }

    public DuplicateEmailException(Email email)
        : base($"A user with email '{email.Value}' already exists.")
    {
        Email = email;
    }

    public DuplicateEmailException(Email email, Exception innerException)
        : base($"A user with email '{email.Value}' already exists.", innerException)
    {
        Email = email;
    }
}
