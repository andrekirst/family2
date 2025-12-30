using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user is not found in the system.
/// </summary>
public sealed class UserNotFoundException : Exception
{
    public UserId UserId { get; }

    public UserNotFoundException(UserId userId)
        : base($"User with ID {userId.Value} not found.")
    {
        UserId = userId;
    }
}
