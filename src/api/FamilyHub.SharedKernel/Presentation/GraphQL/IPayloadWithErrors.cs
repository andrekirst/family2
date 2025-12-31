namespace FamilyHub.SharedKernel.Presentation.GraphQL;

/// <summary>
/// Interface for GraphQL mutation payloads that support error handling.
/// All mutation payloads should implement this interface to provide consistent error reporting.
/// </summary>
public interface IPayloadWithErrors
{
    /// <summary>
    /// List of errors that occurred during mutation execution.
    /// Null or empty when the mutation succeeded.
    /// </summary>
    IReadOnlyList<UserError>? Errors { get; }

    /// <summary>
    /// Indicates whether the mutation succeeded (no errors).
    /// </summary>
    bool Success { get; }
}
