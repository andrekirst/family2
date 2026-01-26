using FluentValidation;

namespace FamilyHub.Modules.UserProfile.Application.Commands.UpdateUserProfile;

/// <summary>
/// Validator for UpdateUserProfileCommand.
/// Validates that required fields are present.
/// Note: Vogen value objects handle their own validation on creation.
/// </summary>
public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    /// <summary>
    /// Initializes validator rules.
    /// </summary>
    public UpdateUserProfileCommandValidator()
    {
        // DisplayName is required and Vogen validates the value itself
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required.");
    }
}
