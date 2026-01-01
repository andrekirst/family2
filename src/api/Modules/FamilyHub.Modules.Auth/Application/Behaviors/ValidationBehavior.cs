using FluentValidation;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates commands using FluentValidation.
/// Throws ValidationException if validation fails.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // If no validators are registered, skip validation
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Execute all validators in parallel
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        // If there are failures, throw ValidationException
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // Validation passed, continue to next behavior or handler
        return await next(cancellationToken);
    }
}
