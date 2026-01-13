using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user is not found in the system.
/// </summary>
public sealed class UserNotFoundException(UserId userId) : Exception($"User with ID {userId.Value} not found.")
{
    /// <summary>
    /// Gets the ID of the user that was not found.
    /// </summary>
    public UserId UserId { get; } = userId;
}
