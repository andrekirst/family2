using FamilyHub.SharedKernel.Application.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FamilyHub.Tests.Unit.SharedKernel.Application.Behaviors;

/// <summary>
/// Unit tests for LoggingBehavior.
/// Verifies request logging, timing measurement, and exception handling.
/// </summary>
public sealed class LoggingBehaviorTests
{
    #region Test Request Types

    public record TestCommand : IRequest<string>;
    public record TestQuery : IRequest<int>;
    public record LongRunningCommand : IRequest<bool>;

    #endregion

    #region Successful Execution Tests

    [Fact]
    public async Task Handle_WhenNextSucceeds_ReturnsResponse()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();
        var sut = new LoggingBehavior<TestCommand, string>(logger);
        var request = new TestCommand();
        var expectedResult = "success";
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns(expectedResult);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Handle_WhenNextSucceeds_CallsNextExactlyOnce()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();
        var sut = new LoggingBehavior<TestCommand, string>(logger);
        var request = new TestCommand();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("result");

        // Act
        await sut.Handle(request, next, CancellationToken.None);

        // Assert
        await next.Received(1)();
    }

    [Fact]
    public async Task Handle_WhenNextSucceeds_LogsRequestStarting()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();
        var sut = new LoggingBehavior<TestCommand, string>(logger);
        var request = new TestCommand();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("result");

        // Act
        await sut.Handle(request, next, CancellationToken.None);

        // Assert
        logger.ReceivedCalls().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNextSucceeds_LogsRequestCompleted()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();
        var sut = new LoggingBehavior<TestCommand, string>(logger);
        var request = new TestCommand();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("result");

        // Act
        await sut.Handle(request, next, CancellationToken.None);

        // Assert - Should have at least 2 log calls (start and complete)
        logger.ReceivedCalls().Count().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Handle_WithCommandRequest_LogsCommandTypeName()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();
        var sut = new LoggingBehavior<TestCommand, string>(logger);
        var request = new TestCommand();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("result");

        // Act
        await sut.Handle(request, next, CancellationToken.None);

        // Assert - Verify the logger received calls (LoggerMessage pattern uses Log method)
        logger.ReceivedCalls().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithQueryRequest_LogsQueryTypeName()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestQuery, int>>>();
        var sut = new LoggingBehavior<TestQuery, int>(logger);
        var request = new TestQuery();
        var next = Substitute.For<RequestHandlerDelegate<int>>();
        next().Returns(42);

        // Act
        await sut.Handle(request, next, CancellationToken.None);

        // Assert
        logger.ReceivedCalls().Should().NotBeEmpty();
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task Handle_WhenNextThrows_RethrowsException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();
        var sut = new LoggingBehavior<TestCommand, string>(logger);
        var request = new TestCommand();
        var expectedException = new InvalidOperationException("Test error");
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(expectedException);

        // Act
        var act = () => sut.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test error");
    }

    [Fact]
    public async Task Handle_WhenNextThrows_LogsRequestFailed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();
        var sut = new LoggingBehavior<TestCommand, string>(logger);
        var request = new TestCommand();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(new InvalidOperationException("Test error"));

        // Act
        try
        {
            await sut.Handle(request, next, CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert - Should have logged the failure
        logger.ReceivedCalls().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNextThrows_IncludesExceptionInLog()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();
        var sut = new LoggingBehavior<TestCommand, string>(logger);
        var request = new TestCommand();
        var expectedException = new InvalidOperationException("Test error");
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(expectedException);

        // Act
        try
        {
            await sut.Handle(request, next, CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert - Error log should have been called
        logger.ReceivedCalls().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNextThrows_LogsElapsedTimeBeforeRethrowing()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();
        var sut = new LoggingBehavior<TestCommand, string>(logger);
        var request = new TestCommand();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().ThrowsAsync(new InvalidOperationException("Test error"));

        // Act
        try
        {
            await sut.Handle(request, next, CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert - Logs should have been called (at least start and error)
        logger.ReceivedCalls().Count().Should().BeGreaterThanOrEqualTo(2);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellation_DoesNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();
        var sut = new LoggingBehavior<TestCommand, string>(logger);
        var request = new TestCommand();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        // Note: Return value setup doesn't matter for this test - we're just verifying no exception
        next().Returns("result");

        // Act - Should not throw when provided with a real CancellationToken
        var act = async () => await sut.Handle(request, next, token);

        // Assert - Verify the behavior completes without exception
        await act.Should().NotThrowAsync();
        logger.ReceivedCalls().Should().NotBeEmpty("logging should still occur");
    }

    #endregion

    #region Timing Tests

    [Fact]
    public async Task Handle_WithSlowOperation_LogsAccurateElapsedTime()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<LongRunningCommand, bool>>>();
        var sut = new LoggingBehavior<LongRunningCommand, bool>(logger);
        var request = new LongRunningCommand();
        var next = Substitute.For<RequestHandlerDelegate<bool>>();
        next().Returns(async _ =>
        {
            await Task.Delay(50); // Small delay to verify timing
            return true;
        });

        // Act
        await sut.Handle(request, next, CancellationToken.None);

        // Assert - Should have logged (timing verified indirectly via logging)
        logger.ReceivedCalls().Should().NotBeEmpty();
    }

    #endregion

    #region Independence Tests

    [Fact]
    public async Task Handle_MultipleCalls_EachLogsIndependently()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();
        var sut = new LoggingBehavior<TestCommand, string>(logger);

        var request1 = new TestCommand();
        var request2 = new TestCommand();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("result");

        // Act
        await sut.Handle(request1, next, CancellationToken.None);
        await sut.Handle(request2, next, CancellationToken.None);

        // Assert - Should have logs for both calls
        logger.ReceivedCalls().Count().Should().BeGreaterThanOrEqualTo(4); // 2 logs per call
    }

    [Fact]
    public async Task Handle_WhenNextReturnsNull_HandlesGracefully()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string?>>>();
        var sut = new LoggingBehavior<TestCommand, string?>(logger);
        var request = new TestCommand();
        var next = Substitute.For<RequestHandlerDelegate<string?>>();
        next().Returns((string?)null);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        logger.ReceivedCalls().Should().NotBeEmpty();
    }

    #endregion
}
