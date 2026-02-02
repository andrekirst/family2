using FamilyHub.Api.Features.Family.Application.Commands;
using FluentValidation;

namespace FamilyHub.Api.Features.Family.Application.Validators;

/// <summary>
/// Validator for AddFamilyMemberCommand.
/// </summary>
public sealed class AddFamilyMemberCommandValidator : AbstractValidator<AddFamilyMemberCommand>
{
    public AddFamilyMemberCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotNull()
            .WithMessage("Family ID is required");

        RuleFor(x => x.UserIdToAdd)
            .NotNull()
            .WithMessage("User ID is required");
    }
}
