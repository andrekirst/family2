namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CqrsPatterns.Application.Validators;

/// <summary>
/// INTENTIONAL VIOLATION: Validator class that does NOT inherit from AbstractValidator.
/// Used for negative testing of CqrsPatternTests.Validators_ShouldInheritFrom_AbstractValidator
/// </summary>
public sealed class BadCommandValidator
{
    public bool Validate(object command) => true;
}
