using FluentValidation;

namespace FamilyHub.Modules.Auth.Application.Commands.CreateFamily;

/// <summary>
/// Validator for CreateFamilyCommand.
/// Ensures family name meets business requirements.
/// </summary>
public sealed class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Family name is required");

        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage("Family name cannot be null");

        RuleFor(x => x.Name)
            .MinimumLength(1)
            .WithMessage("Family name must be at least 1 character");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Family name cannot exceed 100 characters");

        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Family name cannot be empty or whitespace");
    }
}
