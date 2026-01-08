namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs.Validators;

/// <summary>
/// Validator for CompleteZitadelLoginInput GraphQL input.
/// Validates OAuth 2.0 authorization code and PKCE code verifier.
/// </summary>
public sealed class CompleteZitadelLoginInputValidator : AbstractValidator<CompleteZitadelLoginInput>
{
    public CompleteZitadelLoginInputValidator()
    {
        RuleFor(x => x.AuthorizationCode)
            .NotEmpty()
            .WithMessage("Authorization code is required");

        RuleFor(x => x.CodeVerifier)
            .NotEmpty()
            .WithMessage("Code verifier is required")
            .MinimumLength(43)
            .WithMessage("Code verifier must be at least 43 characters (PKCE requirement)")
            .MaximumLength(128)
            .WithMessage("Code verifier cannot exceed 128 characters (PKCE requirement)");
    }
}
