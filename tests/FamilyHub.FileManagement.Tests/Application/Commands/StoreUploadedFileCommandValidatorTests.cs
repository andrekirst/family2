using FamilyHub.Api.Features.FileManagement.Application.Commands.StoreUploadedFile;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Application.Commands;

public class StoreUploadedFileCommandValidatorTests
{
    private readonly StoreUploadedFileCommandValidator _validator;

    public StoreUploadedFileCommandValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ValidationMessages>>();
        _validator = new StoreUploadedFileCommandValidator(localizer);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenValid()
    {
        // Arrange
        var command = new StoreUploadedFileCommand(new MemoryStream(new byte[] { 1, 2, 3 }), "test-file.pdf")
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenFileStreamNull()
    {
        // Arrange
        var command = new StoreUploadedFileCommand(null!, "test-file.pdf")
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileStream);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenFileNameEmpty()
    {
        // Arrange
        var command = new StoreUploadedFileCommand(new MemoryStream(new byte[] { 1, 2, 3 }), "")
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }
}
