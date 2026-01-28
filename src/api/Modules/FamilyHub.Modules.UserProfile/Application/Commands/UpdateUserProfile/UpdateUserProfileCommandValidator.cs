using FluentValidation;

namespace FamilyHub.Modules.UserProfile.Application.Commands.UpdateUserProfile;

/// <summary>
/// Validator for UpdateUserProfileCommand.
/// Note: Vogen value objects validate themselves during construction via From().
/// </summary>
public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    /// <summary>
    /// Initializes validator rules.
    /// </summary>
    public UpdateUserProfileCommandValidator()
    {
        // IMPORTANT: Do NOT use NotEmpty() or similar rules on Vogen value objects.
        // FluentValidation's NotEmpty() compares against default(T), which causes
        // Vogen's Equals() to access .Value on an uninitialized struct, throwing
        // "Use of uninitialized Value Object" errors.
        //
        // Vogen value objects self-validate during From() construction in the GraphQL
        // mutation layer. If input is invalid, ValueObjectValidationException is thrown
        // there and converted to a GraphQL error via [DefaultMutationErrors].
    }
}
