namespace FamilyHub.SharedKernel.Presentation.GraphQL;

/// <summary>
/// Factory interface for creating GraphQL mutation payloads.
/// Each payload type should have a corresponding factory implementation that knows how to construct success and error payloads.
/// This pattern eliminates reflection and provides compile-time type safety.
/// </summary>
/// <typeparam name="TResult">The type of the mutation result (command result)</typeparam>
/// <typeparam name="TPayload">The payload type this factory creates</typeparam>
public interface IPayloadFactory<TResult, TPayload>
    where TPayload : IPayloadWithErrors
{
    /// <summary>
    /// Creates a success payload from the mutation result.
    /// </summary>
    /// <param name="result">The successful result of the mutation (strongly-typed command result)</param>
    /// <returns>A payload containing the successful result</returns>
    TPayload Success(TResult result);

    /// <summary>
    /// Creates an error payload from a list of errors.
    /// </summary>
    /// <param name="errors">List of errors that occurred during mutation execution</param>
    /// <returns>A payload containing the errors</returns>
    TPayload Error(IReadOnlyList<UserError> errors);
}
