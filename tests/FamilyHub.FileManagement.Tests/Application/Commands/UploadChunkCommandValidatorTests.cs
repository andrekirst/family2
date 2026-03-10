using FamilyHub.Api.Features.FileManagement.Application.Commands.UploadChunk;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Application.Commands;

public class UploadChunkCommandValidatorTests
{
    private readonly UploadChunkCommandValidator _validator;

    public UploadChunkCommandValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ValidationMessages>>();
        _validator = new UploadChunkCommandValidator(localizer);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenValid()
    {
        // Arrange
        var command = new UploadChunkCommand("upload-123", 0, 4096)
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
    public async Task Validate_ShouldFail_WhenUploadIdEmpty()
    {
        // Arrange
        var command = new UploadChunkCommand("", 0, 4096)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UploadId);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenChunkIndexNegative()
    {
        // Arrange
        var command = new UploadChunkCommand("upload-123", -1, 4096)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChunkIndex);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenChunkSizeZero()
    {
        // Arrange
        var command = new UploadChunkCommand("upload-123", 0, 0)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChunkSize);
    }
}
