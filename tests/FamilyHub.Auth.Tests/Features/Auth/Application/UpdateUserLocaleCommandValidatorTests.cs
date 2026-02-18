using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Application.Commands.UpdateUserLocale;
using FluentValidation.TestHelper;

namespace FamilyHub.Auth.Tests.Features.Auth.Application;

public class UpdateUserLocaleCommandValidatorTests
{
    private readonly UpdateUserLocaleCommandValidator _validator = new();

    [Fact]
    public void ValidLocale_En_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateUserLocaleCommand(
            ExternalUserId.From("ext-123"),
            "en");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidLocale_De_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateUserLocaleCommand(
            ExternalUserId.From("ext-123"),
            "de");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyLocale_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateUserLocaleCommand(
            ExternalUserId.From("ext-123"),
            "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Locale)
            .WithErrorMessage("Locale is required");
    }

    [Fact]
    public void InvalidLocale_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateUserLocaleCommand(
            ExternalUserId.From("ext-123"),
            "fr");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Locale)
            .WithErrorMessage("Locale must be one of: en, de");
    }

    // Note: ExternalUserId is a Vogen value object (struct) that prevents
    // default/null construction at compile time, so the validator's NotNull
    // rule cannot be tested in isolation. Vogen enforces this invariant.
}
