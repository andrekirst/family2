using System.Globalization;
using FamilyHub.Api.Features.Auth.Application.Commands.UpdateUserLocale;
using FamilyHub.Api.Resources;
using FamilyHub.TestCommon.Fakes;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace FamilyHub.Auth.Tests.Features.Auth.Application;

public class UpdateUserLocaleCommandValidatorTests
{
    private readonly UpdateUserLocaleCommandValidator _validator;

    public UpdateUserLocaleCommandValidatorTests()
    {
        var localizationOptions = Options.Create(new RequestLocalizationOptions
        {
            SupportedUICultures = [new CultureInfo("en"), new CultureInfo("de")]
        });

        _validator = new UpdateUserLocaleCommandValidator(
            new StubStringLocalizer<ValidationMessages>(),
            localizationOptions);
    }

    [Fact]
    public void ValidLocale_En_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateUserLocaleCommand("en");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidLocale_De_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateUserLocaleCommand("de");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyLocale_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateUserLocaleCommand("");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Locale);
    }

    [Fact]
    public void InvalidLocale_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateUserLocaleCommand("fr");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Locale);
    }

    // Note: UserId is populated by the UserResolutionBehavior pipeline,
    // so user identity validation is handled before the validator runs.
}
