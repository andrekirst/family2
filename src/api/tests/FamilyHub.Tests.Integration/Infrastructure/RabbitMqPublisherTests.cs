using System.Text;
using FamilyHub.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RabbitMQ.Client;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for RabbitMqPublisher.
/// Tests verify real RabbitMQ connectivity, message publishing,
/// and exchange/queue declarations using Testcontainers.
/// </summary>
[Collection("RabbitMQ")]
public class RabbitMqPublisherTests(RabbitMqContainerFixture fixture) : IAsyncLifetime
{
    private RabbitMqPublisher? _publisher;
    private IOptions<RabbitMqSettings>? _options;

    public Task InitializeAsync()
    {
        var settings = new RabbitMqSettings
        {
            Host = fixture.Host,
            Port = fixture.Port,
            Username = fixture.Username,
            Password = fixture.Password,
            VirtualHost = "/",
            ClientProvidedName = "FamilyHub.Tests.Integration",
            DefaultExchange = "family-hub.test.events",
            DeadLetterExchange = "family-hub.test.dlx",
            DeadLetterQueue = "family-hub.test.dlq",
            MaxRetryAttempts = 3,
            RetryBaseDelay = TimeSpan.FromMilliseconds(100),
            RetryMaxDelay = TimeSpan.FromSeconds(1),
            ConnectionTimeout = TimeSpan.FromSeconds(10),
            EnablePublisherConfirms = true
        };

        _options = Options.Create(settings);
        var logger = Substitute.For<ILogger<RabbitMqPublisher>>();
        _publisher = new RabbitMqPublisher(logger, _options);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_publisher != null)
        {
            await _publisher.DisposeAsync();
        }
    }

    [Fact]
    public async Task PublishAsync_ValidMessage_ShouldPublishSuccessfully()
    {
        // Arrange
        const string exchange = "family-hub.test.events";
        const string routingKey = "test.event.created";
        const string message = """{"eventType":"TestEvent","data":"Hello, RabbitMQ!"}""";

        // Act
        var act = async () => await _publisher!.PublishAsync(exchange, routingKey, message);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_MultipleMessages_ShouldPublishAllSuccessfully()
    {
        // Arrange
        const string exchange = "family-hub.test.events";
        const string routingKey = "test.event.batch";
        var messages = Enumerable.Range(1, 10)
            .Select(i => $"{{\"eventType\":\"BatchEvent\",\"index\":{i}}}")
            .ToList();

        // Act
        var publishTasks = messages.Select(msg =>
            _publisher!.PublishAsync(exchange, routingKey, msg));
        var act = async () => await Task.WhenAll(publishTasks);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_EmptyExchange_ShouldThrowArgumentException()
    {
        // Arrange
        const string routingKey = "test.event";
        const string message = """{"data":"test"}""";

        // Act
        var act = async () => await _publisher!.PublishAsync(string.Empty, routingKey, message);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PublishAsync_EmptyRoutingKey_ShouldThrowArgumentException()
    {
        // Arrange
        const string exchange = "family-hub.test.events";
        const string message = """{"data":"test"}""";

        // Act
        var act = async () => await _publisher!.PublishAsync(exchange, string.Empty, message);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PublishAsync_EmptyMessage_ShouldThrowArgumentException()
    {
        // Arrange
        const string exchange = "family-hub.test.events";
        const string routingKey = "test.event";

        // Act
        var act = async () => await _publisher!.PublishAsync(exchange, routingKey, string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PublishAsync_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        const string exchange = "family-hub.test.events";
        const string routingKey = "test.event";
        const string message = """{"data":"test"}""";

        await _publisher!.DisposeAsync();

        // Act
        var act = async () => await _publisher!.PublishAsync(exchange, routingKey, message);

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task PublishAsync_MessageIsReceivable_ShouldBeConsumedFromQueue()
    {
        // Arrange
        const string exchange = "family-hub.test.events";
        const string routingKey = "test.consumable.event";
        const string queueName = "test.consumable.queue";
        const string expectedMessage = """{"eventType":"ConsumableEvent","value":42}""";

        // Create a temporary consumer to verify message delivery
        var factory = new ConnectionFactory
        {
            HostName = fixture.Host,
            Port = fixture.Port,
            UserName = fixture.Username,
            Password = fixture.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        // Declare queue and bind to exchange
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: true);

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchange,
            routingKey: routingKey);

        // Act - Publish the message
        await _publisher!.PublishAsync(exchange, routingKey, expectedMessage);

        // Allow time for message delivery
        await Task.Delay(500);

        // Assert - Consume and verify the message
        var result = await channel.BasicGetAsync(queueName, autoAck: true);

        result.Should().NotBeNull();
        var receivedMessage = Encoding.UTF8.GetString(result.Body.ToArray());
        receivedMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task PublishAsync_DeclaresDeadLetterExchangeAndQueue()
    {
        // Arrange - Publish a message first to trigger exchange/queue declaration
        const string exchange = "family-hub.test.events";
        const string routingKey = "test.dlx.check";
        const string message = """{"eventType":"DlxCheckEvent"}""";

        await _publisher!.PublishAsync(exchange, routingKey, message);

        // Act - Verify DLX and DLQ exist by attempting to declare them passively
        var factory = new ConnectionFactory
        {
            HostName = fixture.Host,
            Port = fixture.Port,
            UserName = fixture.Username,
            Password = fixture.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        // Assert - Passive declaration succeeds if exchange/queue exists
        var declareExchangeAct = async () => await channel.ExchangeDeclarePassiveAsync(
            _options!.Value.DeadLetterExchange);
        var declareQueueAct = async () => await channel.QueueDeclarePassiveAsync(
            _options!.Value.DeadLetterQueue);

        await declareExchangeAct.Should().NotThrowAsync();
        await declareQueueAct.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_ConcurrentPublishing_ShouldHandleThreadSafely()
    {
        // Arrange
        const string exchange = "family-hub.test.events";
        const int concurrentTasks = 20;
        var tasks = new List<Task>();

        // Act - Publish messages concurrently
        for (var i = 0; i < concurrentTasks; i++)
        {
            var routingKey = $"test.concurrent.{i}";
            var message = $"{{\"index\":{i}}}";
            tasks.Add(_publisher!.PublishAsync(exchange, routingKey, message));
        }

        var act = async () => await Task.WhenAll(tasks);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
