namespace FamilyHub.SharedKernel.Presentation.GraphQL;

/// <summary>
/// Handler interface for executing GraphQL mutations with centralized error handling.
/// Provides consistent exception-to-payload error mapping across all mutations.
/// </summary>
public interface IMutationHandler
{
    /// <summary>
    /// Executes a mutation action and handles exceptions, converting them to GraphQL payload errors.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the mutation action (command result)</typeparam>
    /// <typeparam name="TPayload">The payload type to return</typeparam>
    /// <param name="action">The mutation logic to execute</param>
    /// <returns>A payload containing either the successful result or error information</returns>
    Task<TPayload> Handle<TResult, TPayload>(Func<Task<TResult>> action)
        where TPayload : IPayloadWithErrors;
}
