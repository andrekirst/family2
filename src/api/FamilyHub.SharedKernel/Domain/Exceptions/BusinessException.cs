namespace FamilyHub.SharedKernel.Domain.Exceptions;

/// <summary>
/// Base exception for business logic violations.
/// Used to represent domain-specific errors that can be mapped to user-friendly error messages in GraphQL mutations.
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// Machine-readable error code (e.g., "USER_NOT_FOUND", "VALIDATION_ERROR").
    /// This code is exposed to clients via GraphQL UserError type.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Creates a new BusinessException with the specified error code and message.
    /// </summary>
    /// <param name="code">Machine-readable error code</param>
    /// <param name="message">Human-readable error message</param>
    public BusinessException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// Creates a new BusinessException with the specified error code, message, and inner exception.
    /// </summary>
    /// <param name="code">Machine-readable error code</param>
    /// <param name="message">Human-readable error message</param>
    /// <param name="innerException">The exception that caused this exception</param>
    public BusinessException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}
