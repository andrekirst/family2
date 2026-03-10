using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Common.Modules;
using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that runs FluentValidation validators before the handler.
/// Validators are partitioned into sequential groups by marker interface:
///   1. Input (IInputValidator) — sync format/schema checks
///   2. Auth (IAuthValidator) — async permission checks
///   3. Business (IBusinessValidator) — async existence/uniqueness checks
///   4. Undecorated — validators without a marker (backward-compatible)
///
/// Groups run sequentially with short-circuit: if any group produces failures,
/// subsequent groups are skipped. Within each group, validators run in parallel.
/// Each failure is stamped with ValidatorCategory in CustomState for error filtering.
/// </summary>
[PipelinePriority(PipelinePriorities.Validation)]
public sealed class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var validatorList = validators.ToList();
        if (validatorList.Count == 0)
        {
            return await next(message, cancellationToken);
        }

        // Partition validators into groups by marker interface
        var inputValidators = new List<IValidator<TMessage>>();
        var authValidators = new List<IValidator<TMessage>>();
        var businessValidators = new List<IValidator<TMessage>>();
        var undecoratedValidators = new List<IValidator<TMessage>>();

        foreach (var validator in validatorList)
        {
            var validatorType = validator.GetType();
            if (HasMarkerInterface(validatorType, typeof(IInputValidator<>)))
                inputValidators.Add(validator);
            else if (HasMarkerInterface(validatorType, typeof(IAuthValidator<>)))
                authValidators.Add(validator);
            else if (HasMarkerInterface(validatorType, typeof(IBusinessValidator<>)))
                businessValidators.Add(validator);
            else
                undecoratedValidators.Add(validator);
        }

        // Run groups sequentially with short-circuit
        var context = new ValidationContext<TMessage>(message);

        var failures = await RunGroupAsync(inputValidators, context, ValidatorCategory.Input, cancellationToken);
        if (failures.Count > 0) throw new ValidationException(failures);

        failures = await RunGroupAsync(authValidators, context, ValidatorCategory.Auth, cancellationToken);
        if (failures.Count > 0) throw new ValidationException(failures);

        failures = await RunGroupAsync(businessValidators, context, ValidatorCategory.Business, cancellationToken);
        if (failures.Count > 0) throw new ValidationException(failures);

        // Undecorated validators run last (backward-compatible, stamped as Input)
        failures = await RunGroupAsync(undecoratedValidators, context, ValidatorCategory.Input, cancellationToken);
        if (failures.Count > 0) throw new ValidationException(failures);

        return await next(message, cancellationToken);
    }

    private static async Task<List<ValidationFailure>> RunGroupAsync(
        List<IValidator<TMessage>> group,
        ValidationContext<TMessage> context,
        ValidatorCategory category,
        CancellationToken cancellationToken)
    {
        if (group.Count == 0) return [];

        var results = await Task.WhenAll(
            group.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        // Stamp each failure with the category for error filter mapping
        foreach (var failure in failures)
        {
            failure.CustomState = category;
        }

        return failures;
    }

    private static bool HasMarkerInterface(Type validatorType, Type openGenericMarker)
    {
        return validatorType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericMarker);
    }
}
