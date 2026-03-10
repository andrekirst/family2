using FamilyHub.Api.Features.FileManagement.Application.Queries.DownloadFile;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Application.Queries;

public class DownloadFileQueryValidatorTests
{
    private readonly DownloadFileQueryValidator _validator;

    public DownloadFileQueryValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ValidationMessages>>();
        _validator = new DownloadFileQueryValidator(localizer);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenValid()
    {
        // Arrange
        var query = new DownloadFileQuery("files/family-123/document.pdf")
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
        var query = new DownloadFileQuery("")
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StorageKey);
    }
}
