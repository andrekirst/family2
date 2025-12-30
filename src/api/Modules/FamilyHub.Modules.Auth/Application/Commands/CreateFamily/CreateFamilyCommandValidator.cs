using FluentValidation;

namespace FamilyHub.Modules.Auth.Application.Commands.CreateFamily;

/// <summary>
/// Validator for CreateFamilyCommand.
/// Note: FamilyName value object handles its own validation via Vogen.
/// UserId validation is handled by ICurrentUserService in the handler (throws UnauthenticatedException if null).
/// This validator is kept for future command-level validations if needed.
/// </summary>
public sealed class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator()
    {
        // FamilyName validation is handled by Vogen
        // UserId extraction and validation is handled by ICurrentUserService in the handler
    }
}
