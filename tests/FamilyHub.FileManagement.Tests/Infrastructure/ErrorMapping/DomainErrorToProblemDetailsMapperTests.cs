using FamilyHub.Api.Common.Infrastructure.ErrorMapping;
using FamilyHub.Common.Application;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Infrastructure.ErrorMapping;

public class DomainErrorToProblemDetailsMapperTests
{
    [Fact]
    public void ToProblemDetails_Validation_Returns400()
    {
        // Arrange
        var error = DomainError.Validation("INVALID_INPUT", "Name is required");

        // Act
        var result = DomainErrorToProblemDetailsMapper.ToProblemDetails(error);

        // Assert
        result.ProblemDetails.Status.Should().Be(400);
        result.ProblemDetails.Type.Should().Be("urn:familyhub:errors:validation");
        result.ProblemDetails.Title.Should().Be("Validation Error");
        result.ProblemDetails.Detail.Should().Be("Name is required");
    }

    [Fact]
    public void ToProblemDetails_NotFound_Returns404()
    {
        // Arrange
        var error = DomainError.NotFound("FILE_NOT_FOUND", "File not found");

        // Act
        var result = DomainErrorToProblemDetailsMapper.ToProblemDetails(error);

        // Assert
        result.ProblemDetails.Status.Should().Be(404);
        result.ProblemDetails.Type.Should().Be("urn:familyhub:errors:not-found");
        result.ProblemDetails.Title.Should().Be("Not Found");
        result.ProblemDetails.Detail.Should().Be("File not found");
    }

    [Fact]
    public void ToProblemDetails_Forbidden_Returns403()
    {
        // Arrange
        var error = DomainError.Forbidden("ACCESS_DENIED", "You do not have access");

        // Act
        var result = DomainErrorToProblemDetailsMapper.ToProblemDetails(error);

        // Assert
        result.ProblemDetails.Status.Should().Be(403);
        result.ProblemDetails.Type.Should().Be("urn:familyhub:errors:forbidden");
        result.ProblemDetails.Title.Should().Be("Forbidden");
        result.ProblemDetails.Detail.Should().Be("You do not have access");
    }

    [Fact]
    public void ToProblemDetails_Conflict_Returns409()
    {
        // Arrange
        var error = DomainError.Conflict("DUPLICATE_FILE", "File already exists");

        // Act
        var result = DomainErrorToProblemDetailsMapper.ToProblemDetails(error);

        // Assert
        result.ProblemDetails.Status.Should().Be(409);
        result.ProblemDetails.Type.Should().Be("urn:familyhub:errors:conflict");
        result.ProblemDetails.Title.Should().Be("Conflict");
        result.ProblemDetails.Detail.Should().Be("File already exists");
    }

    [Fact]
    public void ToProblemDetails_BusinessRule_Returns422()
    {
        // Arrange
        var error = DomainError.BusinessRule("QUOTA_EXCEEDED", "Storage quota exceeded");

        // Act
        var result = DomainErrorToProblemDetailsMapper.ToProblemDetails(error);

        // Assert
        result.ProblemDetails.Status.Should().Be(422);
        result.ProblemDetails.Type.Should().Be("urn:familyhub:errors:business-rule");
        result.ProblemDetails.Title.Should().Be("Business Rule Violation");
        result.ProblemDetails.Detail.Should().Be("Storage quota exceeded");
    }

    [Fact]
    public void ToProblemDetails_ShouldIncludeErrorCodeInExtensions()
    {
        // Arrange
        var error = DomainError.NotFound("FILE_NOT_FOUND", "File not found");

        // Act
        var result = DomainErrorToProblemDetailsMapper.ToProblemDetails(error);

        // Assert
        result.ProblemDetails.Extensions.Should().ContainKey("errorCode");
        result.ProblemDetails.Extensions["errorCode"].Should().Be("FILE_NOT_FOUND");
    }
}
