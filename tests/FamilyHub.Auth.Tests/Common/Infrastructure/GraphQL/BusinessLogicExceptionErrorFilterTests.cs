using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentAssertions;
using HotChocolate;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace FamilyHub.Auth.Tests.Common.Infrastructure.GraphQL;

public class BusinessLogicExceptionErrorFilterTests
{
    private static BusinessLogicExceptionErrorFilter CreateFilter(string environmentName)
    {
        var env = Substitute.For<IWebHostEnvironment>();
        env.EnvironmentName.Returns(environmentName);

        var localizer = Substitute.For<IStringLocalizer<DomainErrors>>();
        // Default: return ResourceNotFound=true so fallback to exception message
        localizer[Arg.Any<string>()].Returns(callInfo =>
        {
            var key = callInfo.Arg<string>();
            return new LocalizedString(key, key, resourceNotFound: true);
        });

        return new BusinessLogicExceptionErrorFilter(localizer, env);
    }

    private static IError CreateErrorWithException(Exception exception)
    {
        return ErrorBuilder.New()
            .SetMessage("Unexpected Execution Error")
            .SetException(exception)
            .Build();
    }

    [Fact]
    public void Should_show_original_message_for_InvalidOperationException_in_development()
    {
        var filter = CreateFilter(Environments.Development);
        var error = CreateErrorWithException(
            new InvalidOperationException("Detailed internal error info"));

        var result = filter.OnError(error);

        result.Message.Should().Be("Detailed internal error info");
        result.Code.Should().Be("BUSINESS_LOGIC_ERROR");
    }

    [Fact]
    public void Should_hide_message_for_InvalidOperationException_in_production()
    {
        var filter = CreateFilter(Environments.Production);
        var error = CreateErrorWithException(
            new InvalidOperationException("Sensitive internal details"));

        var result = filter.OnError(error);

        result.Message.Should().Be("An internal error occurred.");
        result.Code.Should().Be("BUSINESS_LOGIC_ERROR");
    }

    [Fact]
    public void Should_hide_message_for_InvalidOperationException_in_staging()
    {
        var filter = CreateFilter(Environments.Staging);
        var error = CreateErrorWithException(
            new InvalidOperationException("Sensitive internal details"));

        var result = filter.OnError(error);

        result.Message.Should().Be("An internal error occurred.");
        result.Code.Should().Be("BUSINESS_LOGIC_ERROR");
    }

    [Fact]
    public void Should_map_DomainException_with_error_code()
    {
        var filter = CreateFilter(Environments.Production);
        var error = CreateErrorWithException(
            new DomainException("Invitation has expired", "INVITATION_EXPIRED"));

        var result = filter.OnError(error);

        result.Message.Should().Be("Invitation has expired");
        result.Code.Should().Be("BUSINESS_LOGIC_ERROR");
    }

    [Fact]
    public void Should_map_ConflictException()
    {
        var filter = CreateFilter(Environments.Production);
        var error = CreateErrorWithException(new ConflictException("Family"));

        var result = filter.OnError(error);

        result.Code.Should().Be("CONFLICT");
        result.Message.Should().Contain("Family");
    }

    [Fact]
    public void Should_pass_through_unhandled_exceptions()
    {
        var filter = CreateFilter(Environments.Production);
        var error = CreateErrorWithException(new ArgumentException("some arg error"));

        var result = filter.OnError(error);

        // Unhandled exceptions are returned as-is
        result.Message.Should().Be("Unexpected Execution Error");
    }

    [Fact]
    public void Should_use_localized_message_when_available()
    {
        var env = Substitute.For<IWebHostEnvironment>();
        env.EnvironmentName.Returns(Environments.Production);

        var localizer = Substitute.For<IStringLocalizer<DomainErrors>>();
        localizer["INVITATION_EXPIRED"].Returns(
            new LocalizedString("INVITATION_EXPIRED", "Die Einladung ist abgelaufen.", resourceNotFound: false));

        var filter = new BusinessLogicExceptionErrorFilter(localizer, env);
        var error = CreateErrorWithException(
            new DomainException("Invitation has expired", "INVITATION_EXPIRED"));

        var result = filter.OnError(error);

        result.Message.Should().Be("Die Einladung ist abgelaufen.");
    }
}
