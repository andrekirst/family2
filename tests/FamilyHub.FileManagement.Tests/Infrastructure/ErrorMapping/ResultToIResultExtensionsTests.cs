using FamilyHub.Api.Common.Infrastructure.ErrorMapping;
using FamilyHub.Common.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Infrastructure.ErrorMapping;

public class ResultToIResultExtensionsTests
{
    [Fact]
    public void ToHttpResult_ShouldCallOnSuccess_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result<string>.Success("ok");

        // Act
        var httpResult = result.ToHttpResult(v => Results.Ok(v));

        // Assert
        httpResult.Should().BeOfType<Ok<string>>();
        var okResult = (Ok<string>)httpResult;
        okResult.Value.Should().Be("ok");
    }

    [Fact]
    public void ToHttpResult_ShouldReturnProblemDetails_WhenResultIsFailure()
    {
        // Arrange
        var result = Result<string>.Failure(DomainError.NotFound("CODE", "msg"));

        // Act
        var httpResult = result.ToHttpResult(v => Results.Ok(v));

        // Assert
        httpResult.Should().BeOfType<ProblemHttpResult>();
        var problemResult = (ProblemHttpResult)httpResult;
        problemResult.ProblemDetails.Status.Should().Be(404);
    }
}
