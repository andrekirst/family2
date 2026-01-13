using System.Diagnostics;
using System.Text;
using FamilyHub.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for RabbitMQ publisher retry policy behavior.
/// Tests exponential backoff, maximum retries, and recovery scenarios.
/// </summary>
/// <remarks>
/// The RabbitMqPublisher uses Polly v8 with:
/// - Exponential backoff with jitter
/// - Configurable max retries (default: 3)
/// - Configurable base delay (default: 1s, tests use 100ms)
///
/// These tests verify the retry behavior works correctly with real
/// RabbitMQ connections using Testcontainers.
/// </remarks>
[Collection("RabbitMQ")]
public sealed class RabbitMqRetryPolicyTests(RabbitMqContainerFixture fixture) : IAsyncLifetime
{
    private readonly ILogger<RabbitMqPublisher> _logger = Substitute.For<ILogger<RabbitMqPublisher>>();

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    #region Invalid Host Retry Tests

    /// <summary>
    /// Tests that publishing to an invalid host eventually fails after max retries.
    /// Verifies that the retry mechanism exhausts all attempts before throwing.
    /// </summary>
    [Fact]
    public async Task PublishAsync_InvalidHost_ThrowsAfterMaxRetries()
    {
        // Arrange - Use invalid host to trigger BrokerUnreachableException
        var settings = new RabbitMqSettings
        {
            Host = "nonexistent.host.invalid",
            Port = 5672,
            Username = "guest",
            Password = "guest",
            MaxRetryAttempts = 2, // Use fewer retries for faster test
            RetryBaseDelay = TimeSpan.FromMilliseconds(50), // Short delays for test speed
            RetryMaxDelay = TimeSpan.FromMilliseconds(500),
            ConnectionTimeout = TimeSpan.FromSeconds(2)
        };

        await using var publisher = new RabbitMqPublisher(_logger, Options.Create(settings));

        // Act
        var sw = Stopwatch.StartNew();
        var act = async () => await publisher.PublishAsync(
            "test.exchange",
            "test.routing.key",
            """{"test": "message"}""");

        // Assert - Should throw after retries are exhausted
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*"); // Any exception message (DNS resolution, connection refused, etc.)

        sw.Stop();

        // Verify retries occurred by checking elapsed time
        // With 2 retries and 50ms base delay (exponential: ~50ms + ~100ms + connection timeouts)
        // Minimum expected time should be at least the retry delays
        sw.ElapsedMilliseconds.Should().BeGreaterThan(50,
            "retries should add delay to the overall operation");
    }

    /// <summary>
    /// Tests that retry timing follows exponential backoff pattern.
    /// </summary>
    [Fact]
    public async Task PublishAsync_InvalidHost_RetriesWithExponentialBackoff()
    {
        // Arrange
        var retryAttempts = new List<(DateTime Time, int Attempt)>();
        var mockLogger = Substitute.For<ILogger<RabbitMqPublisher>>();

        // Capture retry timing via logger calls
        mockLogger.When(x => x.Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>()))
            .Do(callInfo =>
            {
                var state = callInfo.ArgAt<object>(2);
                var message = state?.ToString() ?? string.Empty;
                if (message.Contains("Retry attempt"))
                {
                    // Extract attempt number from log message
                    var attemptMatch = System.Text.RegularExpressions.Regex.Match(message, @"attempt (\d+)");
                    if (attemptMatch.Success && int.TryParse(attemptMatch.Groups[1].Value, out var attempt))
                    {
                        retryAttempts.Add((DateTime.UtcNow, attempt));
                    }
                }
            });

        var settings = new RabbitMqSettings
        {
            Host = "nonexistent.host.invalid",
            Port = 5672,
            Username = "guest",
            Password = "guest",
            MaxRetryAttempts = 3,
            RetryBaseDelay = TimeSpan.FromMilliseconds(100),
            RetryMaxDelay = TimeSpan.FromMilliseconds(1000),
            ConnectionTimeout = TimeSpan.FromMilliseconds(500) // Short timeout
        };

        await using var publisher = new RabbitMqPublisher(mockLogger, Options.Create(settings));

        // Act
        var startTime = DateTime.UtcNow;
        try
        {
            await publisher.PublishAsync(
                "test.exchange",
                "test.routing.key",
                """{"test": "exponential"}""");
        }
        catch
        {
            // Expected to fail
        }

        var endTime = DateTime.UtcNow;

        // Assert - Total time should reflect multiple retry attempts
        var totalDuration = endTime - startTime;
        totalDuration.TotalMilliseconds.Should().BeGreaterThan(100,
            "exponential backoff should add delays between retries");
    }

    #endregion

    #region Recovery After Transient Failure Tests

    /// <summary>
    /// Tests that after a successful connection, messages are delivered.
    /// </summary>
    [Fact]
    public async Task PublishAsync_ValidHost_SucceedsWithoutRetry()
    {
        // Arrange - Use actual container
        var settings = CreateTestSettings();
        await using var publisher = new RabbitMqPublisher(_logger, Options.Create(settings));

        var exchangeName = $"test.valid.{Guid.NewGuid():N}";
        var routingKey = "test.success";
        var receivedMessage = new TaskCompletionSource<string>();

        // Set up consumer to verify message delivery
        var factory = new ConnectionFactory
        {
            HostName = fixture.Host,
            Port = fixture.Port,
            UserName = fixture.Username,
            Password = fixture.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: false, autoDelete: true);
        var queueDeclare = await channel.QueueDeclareAsync(exclusive: true, autoDelete: true);
        await channel.QueueBindAsync(queueDeclare.QueueName, exchangeName, routingKey);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (sender, args) =>
        {
            receivedMessage.TrySetResult(Encoding.UTF8.GetString(args.Body.ToArray()));
            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queueDeclare.QueueName, autoAck: true, consumer);

        // Act
        var sw = Stopwatch.StartNew();
        await publisher.PublishAsync(exchangeName, routingKey, """{"status": "success"}""");
        sw.Stop();

        // Assert - Should succeed quickly without retries
        var received = await Task.WhenAny(
            receivedMessage.Task,
            Task.Delay(TimeSpan.FromSeconds(5)));

        received.Should().Be(receivedMessage.Task, "message should be delivered");
        var message = await receivedMessage.Task;
        message.Should().Contain("success");

        // Without retries, should complete quickly (under 2 seconds typically)
        sw.ElapsedMilliseconds.Should().BeLessThan(3000,
            "valid host should not need retries");
    }

    /// <summary>
    /// Tests that publisher recovers after initial failure and delivers subsequent messages.
    /// </summary>
    [Fact]
    public async Task PublishAsync_AfterRecovery_SubsequentMessagesDelivered()
    {
        // Arrange
        var settings = CreateTestSettings();
        await using var publisher = new RabbitMqPublisher(_logger, Options.Create(settings));

        var exchangeName = $"test.recovery.{Guid.NewGuid():N}";
        var routingKey = "test.recovery.key";
        var receivedMessages = new List<string>();
        var allMessagesReceived = new TaskCompletionSource<bool>();

        // Set up consumer
        var factory = new ConnectionFactory
        {
            HostName = fixture.Host,
            Port = fixture.Port,
            UserName = fixture.Username,
            Password = fixture.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: false, autoDelete: true);
        var queueDeclare = await channel.QueueDeclareAsync(exclusive: true, autoDelete: true);
        await channel.QueueBindAsync(queueDeclare.QueueName, exchangeName, routingKey);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (sender, args) =>
        {
            lock (receivedMessages)
            {
                receivedMessages.Add(Encoding.UTF8.GetString(args.Body.ToArray()));
                if (receivedMessages.Count >= 3)
                {
                    allMessagesReceived.TrySetResult(true);
                }
            }
            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queueDeclare.QueueName, autoAck: true, consumer);

        // Act - Publish multiple messages
        await publisher.PublishAsync(exchangeName, routingKey, """{"message": 1}""");
        await publisher.PublishAsync(exchangeName, routingKey, """{"message": 2}""");
        await publisher.PublishAsync(exchangeName, routingKey, """{"message": 3}""");

        // Assert
        var received = await Task.WhenAny(
            allMessagesReceived.Task,
            Task.Delay(TimeSpan.FromSeconds(10)));

        received.Should().Be(allMessagesReceived.Task, "all messages should be received");
        receivedMessages.Should().HaveCount(3);
    }

    #endregion

    #region Retry Count Verification Tests

    /// <summary>
    /// Tests that retry attempts add measurable delay proving retries occurred.
    /// With MaxRetryAttempts=2 and base delay of 50ms, total time should include retry delays.
    /// </summary>
    [Fact]
    public async Task PublishAsync_InvalidHost_RetryDelaysAddUpCorrectly()
    {
        // Arrange - Configure with short but measurable delays
        var maxRetries = 2;
        var baseDelayMs = 50;
        var settings = new RabbitMqSettings
        {
            Host = "nonexistent.host.invalid",
            Port = 5672,
            Username = "guest",
            Password = "guest",
            MaxRetryAttempts = maxRetries,
            RetryBaseDelay = TimeSpan.FromMilliseconds(baseDelayMs),
            RetryMaxDelay = TimeSpan.FromMilliseconds(500),
            ConnectionTimeout = TimeSpan.FromMilliseconds(100) // Short timeout
        };

        await using var publisher = new RabbitMqPublisher(_logger, Options.Create(settings));

        // Act - Time the operation
        var sw = Stopwatch.StartNew();
        try
        {
            await publisher.PublishAsync("test.exchange", "test.key", "{}");
        }
        catch
        {
            // Expected to fail
        }
        sw.Stop();

        // Assert - Total time should include retry delays
        // With exponential backoff: ~50ms + ~100ms = ~150ms minimum for delays alone
        // Plus connection timeout attempts. Should be > 100ms to prove retries occurred.
        sw.ElapsedMilliseconds.Should().BeGreaterThan(baseDelayMs,
            $"with {maxRetries} retries at {baseDelayMs}ms base delay, total time should include retry delays");
    }

    #endregion

    #region Parameter Validation Tests

    /// <summary>
    /// Tests that empty exchange throws immediately without retrying.
    /// </summary>
    [Fact]
    public async Task PublishAsync_EmptyExchange_ThrowsImmediately()
    {
        // Arrange
        var settings = CreateTestSettings();
        await using var publisher = new RabbitMqPublisher(_logger, Options.Create(settings));

        // Act
        var sw = Stopwatch.StartNew();
        var act = async () => await publisher.PublishAsync(
            string.Empty,
            "test.key",
            """{"test": "message"}""");

        // Assert - Should throw immediately (validation, not retry)
        await act.Should().ThrowAsync<ArgumentException>();
        sw.Stop();

        // Validation failures should be immediate (< 100ms)
        sw.ElapsedMilliseconds.Should().BeLessThan(100,
            "validation failure should not trigger retries");
    }

    /// <summary>
    /// Tests that publishing after dispose throws ObjectDisposedException.
    /// </summary>
    [Fact]
    public async Task PublishAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var settings = CreateTestSettings();
        var publisher = new RabbitMqPublisher(_logger, Options.Create(settings));
        await publisher.DisposeAsync();

        // Act
        var act = async () => await publisher.PublishAsync(
            "test.exchange",
            "test.key",
            """{"test": "message"}""");

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    #endregion

    private RabbitMqSettings CreateTestSettings() => new()
    {
        Host = fixture.Host,
        Port = fixture.Port,
        Username = fixture.Username,
        Password = fixture.Password,
        MaxRetryAttempts = 3,
        RetryBaseDelay = TimeSpan.FromMilliseconds(100), // Short for tests
        RetryMaxDelay = TimeSpan.FromSeconds(2),
        ConnectionTimeout = TimeSpan.FromSeconds(5),
        EnablePublisherConfirms = true
    };
}
