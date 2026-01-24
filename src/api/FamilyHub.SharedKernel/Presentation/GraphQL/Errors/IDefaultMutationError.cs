namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Marker interface for error types that should be automatically
/// registered on all GraphQL mutations.
/// </summary>
/// <remarks>
/// <para>
/// Error types implementing this interface will be automatically added
/// to every mutation's error union by <c>DefaultMutationErrorsInterceptor</c>.
/// </para>
/// <para>
/// To add a new default error type, simply create a class that:
/// <list type="number">
/// <item><description>Inherits from <see cref="BaseError"/></description></item>
/// <item><description>Implements <see cref="IDefaultMutationError"/></description></item>
/// </list>
/// No other changes are required - the interceptor discovers types automatically.
/// </para>
/// <para>
/// Mutation-specific errors can still be declared using <c>[Error(typeof(...))]</c>
/// attributes; they will be added alongside the default errors.
/// </para>
/// </remarks>
public interface IDefaultMutationError;
