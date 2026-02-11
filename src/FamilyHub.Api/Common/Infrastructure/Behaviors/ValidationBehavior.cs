using FamilyHub.Api.Common.Modules;
using FluentValidation;
using Mediator;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that runs FluentValidation validators before the handler.
/// Throws ValidationException if any validators fail.
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
        if (validators.Any())
        {
            var context = new ValidationContext<TMessage>(message);

            var results = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = results
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count > 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next(message, cancellationToken);
    }
}
