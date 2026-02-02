using FamilyHub.Api.Features.Family.Application.Commands;
using FluentValidation;

namespace FamilyHub.Api.Features.Family.Application.Validators;

/// <summary>
/// Validator for CreateFamilyCommand.
/// Note: Vogen value objects already enforce basic validation,
/// this validator provides additional business rules.
/// </summary>
public sealed class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage("Family name is required");

        RuleFor(x => x.OwnerId)
            .NotNull()
            .WithMessage("Owner ID is required");
    }
}
