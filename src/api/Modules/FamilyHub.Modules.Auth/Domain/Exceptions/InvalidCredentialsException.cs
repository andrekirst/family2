namespace FamilyHub.Modules.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when login credentials are invalid.
/// </summary>
public sealed class InvalidCredentialsException() : Exception("Invalid email or password.");
