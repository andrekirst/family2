using FamilyHub.SharedKernel.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FamilyHub.SharedKernel.Presentation.GraphQL;

/// <summary>
/// Centralized mutation handler that executes GraphQL mutation logic and handles exceptions,
/// converting them to user-friendly GraphQL payload errors.
/// Provides consistent error handling, logging, and payload generation across all mutations.
/// </summary>
public class MutationHandler(IServiceProvider services, ILogger<MutationHandler> logger) : IMutationHandler
{
    /// <summary>
    /// Executes a mutation action and handles exceptions, converting them to GraphQL payload errors.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the mutation action (command result)</typeparam>
    /// <typeparam name="TPayload">The payload type to return</typeparam>
    /// <param name="action">The mutation logic to execute</param>
    /// <returns>A payload containing either the successful result or error information</returns>
    public async Task<TPayload> Handle<TResult, TPayload>(
        Func<Task<TResult>> action)
        where TPayload : IPayloadWithErrors
    {
        try
        {
            // Execute the mutation logic
            var result = await action();

            // Resolve the payload factory from DI and create success payload
            var factory = services.GetRequiredService<IPayloadFactory<TResult, TPayload>>();
            return factory.Success(result);
        }
        catch (BusinessException ex)
        {
            // Business exceptions represent expected domain violations
            // Log as Warning since these are not system errors
            logger.LogWarning(ex,
                "Business exception in {PayloadType}: {Code} - {Message}",
                typeof(TPayload).Name, ex.Code, ex.Message);

            var factory = services.GetRequiredService<IPayloadFactory<TResult, TPayload>>();
            return factory.Error([
                new UserError
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    Field = null
                }
            ]);
        }
        catch (ValidationException validationEx)
        {
            // FluentValidation exceptions contain multiple validation errors
            // Log as Warning with error count
            logger.LogWarning(
                "Validation failed in {PayloadType}: {ErrorCount} errors",
                typeof(TPayload).Name, validationEx.Errors.Count());

            var factory = services.GetRequiredService<IPayloadFactory<TResult, TPayload>>();
            var errors = validationEx.Errors.Select(e => new UserError
            {
                Code = "VALIDATION_ERROR",
                Message = e.ErrorMessage,
                Field = e.PropertyName
            }).ToList();

            return factory.Error(errors);
        }
        catch (ValueObjectValidationException vogenEx)
        {
            // Vogen validation exceptions occur when value object creation fails
            // Log as Warning with the validation message
            logger.LogWarning(vogenEx,
                "Vogen validation failed in {PayloadType}: {Message}",
                typeof(TPayload).Name, vogenEx.Message);

            var factory = services.GetRequiredService<IPayloadFactory<TResult, TPayload>>();
            return factory.Error([
                new UserError
                {
                    Code = "VALIDATION_ERROR",
                    Message = vogenEx.Message,
                    Field = null
                }
            ]);
        }
        catch (UnauthorizedAccessException unauthorizedEx)
        {
            // Authentication/authorization exceptions represent missing or invalid credentials
            // Log as Warning since these are expected when users aren't authenticated
            logger.LogWarning(unauthorizedEx,
                "Unauthorized access in {PayloadType}: {Message}",
                typeof(TPayload).Name, unauthorizedEx.Message);

            var factory = services.GetRequiredService<IPayloadFactory<TResult, TPayload>>();
            return factory.Error([
                new UserError
                {
                    Code = "UNAUTHENTICATED",
                    Message = unauthorizedEx.Message,
                    Field = null
                }
            ]);
        }
        catch (Exception ex)
        {
            // Unexpected exceptions represent system errors
            // Log as Error with full exception details for debugging
            logger.LogError(ex,
                "Unexpected exception in {PayloadType}: {ExceptionType}",
                typeof(TPayload).Name, ex.GetType().Name);

            var factory = services.GetRequiredService<IPayloadFactory<TResult, TPayload>>();
            return factory.Error([
                new UserError
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An unexpected error occurred. Please try again.",
                    Field = null
                }
            ]);
        }
    }
}
