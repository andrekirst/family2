using FluentValidation;
using FamilyHub.Api.Features.EventChain.Application.Commands;

namespace FamilyHub.Api.Features.EventChain.Application.Validators;

public sealed class CreateChainDefinitionCommandValidator : AbstractValidator<CreateChainDefinitionCommand>
{
    public CreateChainDefinitionCommandValidator()
    {
        RuleFor(x => x.Name.Value)
            .NotEmpty().WithMessage("Chain name is required")
            .MaximumLength(200).WithMessage("Chain name cannot exceed 200 characters");

        RuleFor(x => x.TriggerEventType)
            .NotEmpty().WithMessage("Trigger event type is required");

        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage("At least one step is required");

        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.Alias.Value)
                .NotEmpty().WithMessage("Step alias is required")
                .MaximumLength(50).WithMessage("Step alias cannot exceed 50 characters");

            step.RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Step name is required");

            step.RuleFor(s => s.ActionType)
                .NotEmpty().WithMessage("Action type is required");

            step.RuleFor(s => s.Order)
                .GreaterThan(0).WithMessage("Step order must be positive");
        });
    }
}
