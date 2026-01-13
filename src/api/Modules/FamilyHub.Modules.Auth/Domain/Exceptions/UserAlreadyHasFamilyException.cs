using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to create/join a family but already belongs to one.
/// </summary>
public sealed class UserAlreadyHasFamilyException(UserId userId, int familyCount) : Exception("User already belongs to a family. Users can only be members of one family at a time.")
{
    /// <summary>
    /// Gets the ID of the user who attempted to join another family.
    /// </summary>
    public UserId UserId { get; } = userId;

    /// <summary>
    /// Gets the number of families the user already belongs to.
    /// </summary>
    public int FamilyCount { get; } = familyCount;
}
