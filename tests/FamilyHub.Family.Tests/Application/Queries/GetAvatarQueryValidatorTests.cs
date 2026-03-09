using FamilyHub.Api.Features.Family.Application.Queries.GetAvatar;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace FamilyHub.Family.Tests.Application.Queries;

public class GetAvatarQueryValidatorTests
{
    private readonly GetAvatarQueryValidator _validator;

    public GetAvatarQueryValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ValidationMessages>>();
        _validator = new GetAvatarQueryValidator(localizer);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenValid()
    {
        // Arrange
        var query = new GetAvatarQuery(Guid.NewGuid(), "small") { UserId = UserId.New() };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenAvatarIdEmpty()
    {
        // Arrange
        var query = new GetAvatarQuery(Guid.Empty, "small") { UserId = UserId.New() };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AvatarId);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenSizeEmpty()
    {
        // Arrange
        var query = new GetAvatarQuery(Guid.NewGuid(), "") { UserId = UserId.New() };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Size);
    }
}
