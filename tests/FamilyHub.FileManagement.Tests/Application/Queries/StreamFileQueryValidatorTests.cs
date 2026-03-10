using FamilyHub.Api.Features.FileManagement.Application.Queries.StreamFile;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Application.Queries;

public class StreamFileQueryValidatorTests
{
    private readonly StreamFileQueryValidator _validator;

    public StreamFileQueryValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ValidationMessages>>();
        _validator = new StreamFileQueryValidator(localizer);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenValid()
    {
        // Arrange
        var query = new StreamFileQuery("files/family-123/video.mp4", null, null)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenValidWithRange()
    {
        // Arrange
        var query = new StreamFileQuery("files/family-123/video.mp4", 0, 1024)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenStorageKeyEmpty()
    {
        // Arrange
        var query = new StreamFileQuery("", null, null)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StorageKey);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenRangeFromNegative()
    {
        // Arrange
        var query = new StreamFileQuery("files/family-123/video.mp4", -1, null)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RangeFrom);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenRangeToLessThanRangeFrom()
    {
        // Arrange
        var query = new StreamFileQuery("files/family-123/video.mp4", 100, 50)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RangeTo);
    }
}
