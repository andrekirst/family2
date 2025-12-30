namespace FamilyHub.Modules.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when authentication is required but user is not authenticated.
/// </summary>
public sealed class UnauthenticatedException : Exception
{
    public UnauthenticatedException(string message) : base(message) { }
}
