namespace FamilyHub.SharedKernel.Presentation.GraphQL;

/// <summary>
/// Base record for GraphQL mutation payloads.
/// Provides consistent error handling structure across all mutations.
/// </summary>
public abstract record PayloadBase : IPayloadWithErrors
{
    /// <summary>
    /// List of errors that occurred during mutation execution.
    /// Null or empty when the mutation succeeded.
    /// </summary>
    public IReadOnlyList<UserError>? Errors { get; init; }

    /// <summary>
    /// Default constructor for successful payloads (no errors).
    /// </summary>
    protected PayloadBase()
    {
        Errors = null;
    }

    /// <summary>
    /// Constructor for failed payloads with errors.
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    protected PayloadBase(IReadOnlyList<UserError> errors)
    {
        Errors = errors;
    }

    /// <summary>
    /// Indicates whether the mutation succeeded (no errors).
    /// </summary>
    public bool Success => Errors is null || Errors.Count == 0;
}
