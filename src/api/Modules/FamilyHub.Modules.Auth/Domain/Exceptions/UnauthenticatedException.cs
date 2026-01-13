namespace FamilyHub.Modules.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when authentication is required but user is not authenticated.
/// </summary>
public sealed class UnauthenticatedException(string message) : Exception(message);
