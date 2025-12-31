using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to create/join a family but already belongs to one.
/// </summary>
public sealed class UserAlreadyHasFamilyException : Exception
{
    public UserId UserId { get; }
    public int FamilyCount { get; }

    public UserAlreadyHasFamilyException(UserId userId, int familyCount)
        : base("User already belongs to a family. Users can only be members of one family at a time.")
    {
        UserId = userId;
        FamilyCount = familyCount;
    }
}
