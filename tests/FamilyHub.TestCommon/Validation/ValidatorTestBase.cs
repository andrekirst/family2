using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;

namespace FamilyHub.TestCommon.Validation;

/// <summary>
/// Base class for validator unit tests providing convenience methods
/// for validating commands and asserting on validation results.
/// </summary>
public abstract class ValidatorTestBase<TValidator, TCommand>
    where TValidator : IValidator<TCommand>
{
    protected abstract TValidator CreateValidator();

    protected async Task<ValidationResult> ValidateAsync(TCommand command)
    {
        var validator = CreateValidator();
        return await validator.ValidateAsync(command);
    }

    protected static void AssertHasError(ValidationResult result, string errorCode)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == errorCode,
            $"expected error code '{errorCode}' but found: [{string.Join(", ", result.Errors.Select(e => e.ErrorCode))}]");
    }

    protected static void AssertHasErrorForProperty(ValidationResult result, string propertyName)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == propertyName,
            $"expected error for property '{propertyName}'");
    }

    protected static void AssertNoErrors(ValidationResult result)
    {
        result.IsValid.Should().BeTrue(
            $"expected no errors but found: [{string.Join(", ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))}]");
    }
}
