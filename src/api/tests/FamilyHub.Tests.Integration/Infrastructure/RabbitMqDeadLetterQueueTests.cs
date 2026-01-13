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
/// Integration tests for RabbitMQ Dead Letter Queue (DLQ) functionality.
/// Tests DLX/DLQ declaration, message routing to DLQ on rejection, and message preservation.
/// </summary>
/// <remarks>
/// The RabbitMqPublisher declares:
/// - family-hub.dlx (Fanout exchange for dead letter routing)
/// - family-hub.dlq (Queue for dead letter messages)
///
/// Messages are routed to DLQ when:
/// - A consumer rejects a message with requeue=false
/// - A message expires (TTL)
/// - A queue reaches max length
/// </remarks>
[Collection("RabbitMQ")]
public sealed class RabbitMqDeadLetterQueueTests(RabbitMqContainerFixture fixture) : IAsyncLifetime
{
    private readonly ILogger<RabbitMqPublisher> _logger = Substitute.For<ILogger<RabbitMqPublisher>>();
    private RabbitMqPublisher _publisher = null!;
    private IConnection? _verificationConnection;
    private IChannel? _verificationChannel;

    public async Task InitializeAsync()
    {
        var settings = CreateTestSettings();
        _publisher = new RabbitMqPublisher(_logger, Options.Create(settings));

        // Create verification connection/channel
        var factory = new ConnectionFactory
        {
            HostName = fixture.Host,
            Port = fixture.Port,
            UserName = fixture.Username,
            Password = fixture.Password
        };

        _verificationConnection = await factory.CreateConnectionAsync();
        _verificationChannel = await _verificationConnection.CreateChannelAsync();

        // Drain any leftover messages in the DLQ from previous tests
        await DrainDlqAsync();
    }

    /// <summary>
    /// Drains all messages from the DLQ to ensure test isolation.
    /// </summary>
    private async Task DrainDlqAsync()
    {
        try
        {
            // First trigger publisher to ensure DLQ exists
            var tempExchange = $"temp.drain.{Guid.NewGuid():N}";
            await _verificationChannel!.ExchangeDeclareAsync(tempExchange, ExchangeType.Topic, durable: false, autoDelete: true);
            await _publisher.PublishAsync(tempExchange, "drain", "{}");

            // Drain all messages
            BasicGetResult? result;
            do
            {
                result = await _verificationChannel.BasicGetAsync("family-hub.dlq", autoAck: true);
            } while (result != null);
        }
        catch
        {
            // DLQ may not exist yet, ignore
        }
    }

    public async Task DisposeAsync()
    {
        await _publisher.DisposeAsync();

        if (_verificationChannel != null)
        {
            await _verificationChannel.CloseAsync();
            await _verificationChannel.DisposeAsync();
        }

        if (_verificationConnection != null)
        {
            await _verificationConnection.CloseAsync();
            await _verificationConnection.DisposeAsync();
        }
    }

    #region DLX/DLQ Infrastructure Declaration Tests

    /// <summary>
    /// Tests that the publisher declares the Dead Letter Exchange on first publish.
    /// </summary>
    [Fact]
    public async Task Publisher_OnFirstPublish_DeclaresDlxExchange()
    {
        // Arrange - Create a temporary exchange for the test message
        var testExchange = $"test.dlx.check.{Guid.NewGuid():N}";
        await _verificationChannel!.ExchangeDeclareAsync(
            testExchange,
            ExchangeType.Topic,
            durable: false,
            autoDelete: true);

        // Act - Publish to trigger exchange declarations
        await _publisher.PublishAsync(testExchange, "test.key", """{"check": "dlx"}""");

        // Assert - Verify DLX exists using passive declare
        var dlxExists = await ExchangeExistsAsync("family-hub.dlx");
        dlxExists.Should().BeTrue("DLX should be declared by publisher on first publish");
    }

    /// <summary>
    /// Tests that the publisher declares the Dead Letter Queue on first publish.
    /// </summary>
    [Fact]
    public async Task Publisher_OnFirstPublish_DeclaresDlqQueue()
    {
        // Arrange - Create a temporary exchange for the test message
        var testExchange = $"test.dlq.check.{Guid.NewGuid():N}";
        await _verificationChannel!.ExchangeDeclareAsync(
            testExchange,
            ExchangeType.Topic,
            durable: false,
            autoDelete: true);

        // Act - Publish to trigger queue declarations
        await _publisher.PublishAsync(testExchange, "test.key", """{"check": "dlq"}""");

        // Assert - Verify DLQ exists using passive declare
        var dlqExists = await QueueExistsAsync("family-hub.dlq");
        dlqExists.Should().BeTrue("DLQ should be declared by publisher on first publish");
    }

    /// <summary>
    /// Tests that DLQ is bound to DLX exchange.
    /// </summary>
    [Fact]
    public async Task Publisher_DlqBoundToDlx_CanRouteMessages()
    {
        // Arrange - Create a test message and trigger initialization
        var testExchange = $"test.dlx.binding.{Guid.NewGuid():N}";
        await _verificationChannel!.ExchangeDeclareAsync(
            testExchange,
            ExchangeType.Topic,
            durable: false,
            autoDelete: true);

        await _publisher.PublishAsync(testExchange, "test.key", """{"init": "true"}""");

        // Act - Publish directly to DLX to verify DLQ binding
        var testMessage = """{"direct": "to_dlx"}""";
        var body = Encoding.UTF8.GetBytes(testMessage);

        await _verificationChannel.BasicPublishAsync(
            exchange: "family-hub.dlx",
            routingKey: string.Empty, // Fanout ignores routing key
            body: body);

        // Wait for message to be routed
        await Task.Delay(500);

        // Assert - Message should appear in DLQ
        var result = await _verificationChannel.BasicGetAsync("family-hub.dlq", autoAck: true);
        result.Should().NotBeNull("message published to DLX should appear in DLQ");

        var receivedMessage = Encoding.UTF8.GetString(result.Body.ToArray());
        receivedMessage.Should().Contain("direct");
    }

    #endregion

    #region Message Rejection to DLQ Tests

    /// <summary>
    /// Tests that a rejected message (nack without requeue) is routed to DLQ.
    /// </summary>
    [Fact]
    public async Task Consumer_RejectsMessage_MessageAppearsInDlq()
    {
        // Arrange - Create queue with DLX routing
        var testQueue = $"test.reject.{Guid.NewGuid():N}";
        var testExchange = $"test.reject.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.reject.key";
        var testMessage = $$$"""{"test": "reject_to_dlq", "id": "{{{Guid.NewGuid()}}}"}""";

        // Declare exchange FIRST (before publishing)
        await _verificationChannel!.ExchangeDeclareAsync(
            testExchange,
            ExchangeType.Topic,
            durable: false,
            autoDelete: true);

        // Trigger publisher to declare DLX/DLQ
        await _publisher.PublishAsync(testExchange, "init.key", """{"init": true}""");

        // Declare queue with dead letter routing to DLX
        await _verificationChannel.QueueDeclareAsync(
            queue: testQueue,
            durable: false,
            exclusive: false,
            autoDelete: true,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "family-hub.dlx"
            });

        await _verificationChannel.QueueBindAsync(testQueue, testExchange, routingKey);

        // Set up consumer that rejects messages
        var consumer = new AsyncEventingBasicConsumer(_verificationChannel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            // Reject without requeue -> routes to DLX
            await _verificationChannel.BasicNackAsync(
                args.DeliveryTag,
                multiple: false,
                requeue: false);
        };

        await _verificationChannel.BasicConsumeAsync(testQueue, autoAck: false, consumer);

        // Act - Publish message that will be rejected
        await _publisher.PublishAsync(testExchange, routingKey, testMessage);

        // Wait for rejection and routing
        await Task.Delay(2000);

        // Assert - Message should appear in DLQ
        var dlqMessage = await _verificationChannel.BasicGetAsync("family-hub.dlq", autoAck: true);
        dlqMessage.Should().NotBeNull("rejected message should be routed to DLQ");

        var receivedBody = Encoding.UTF8.GetString(dlqMessage.Body.ToArray());
        receivedBody.Should().Contain("reject_to_dlq");
    }

    /// <summary>
    /// Tests that rejected message preserves its original body content.
    /// </summary>
    [Fact]
    public async Task DlqMessage_PreservesOriginalBody()
    {
        // Arrange
        var testQueue = $"test.preserve.{Guid.NewGuid():N}";
        var testExchange = $"test.preserve.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.preserve.key";
        var originalMessage = """{"original": "content", "data": [1, 2, 3], "nested": {"key": "value"}}""";

        // Declare exchange FIRST (before publishing)
        await _verificationChannel!.ExchangeDeclareAsync(
            testExchange,
            ExchangeType.Topic,
            durable: false,
            autoDelete: true);

        // Trigger publisher to declare DLX/DLQ
        await _publisher.PublishAsync(testExchange, "init", "{}");

        await _verificationChannel.QueueDeclareAsync(
            queue: testQueue,
            durable: false,
            exclusive: false,
            autoDelete: true,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "family-hub.dlx"
            });

        await _verificationChannel.QueueBindAsync(testQueue, testExchange, routingKey);

        // Reject all messages
        var consumer = new AsyncEventingBasicConsumer(_verificationChannel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            await _verificationChannel.BasicNackAsync(args.DeliveryTag, false, false);
        };

        await _verificationChannel.BasicConsumeAsync(testQueue, autoAck: false, consumer);

        // Act
        await _publisher.PublishAsync(testExchange, routingKey, originalMessage);
        await Task.Delay(2000);

        // Assert
        var dlqMessage = await _verificationChannel.BasicGetAsync("family-hub.dlq", autoAck: true);
        dlqMessage.Should().NotBeNull();

        var receivedBody = Encoding.UTF8.GetString(dlqMessage.Body.ToArray());
        receivedBody.Should().Be(originalMessage, "DLQ should preserve exact message body");
    }

    /// <summary>
    /// Tests that rejected message contains dead-letter headers with routing info.
    /// </summary>
    [Fact]
    public async Task DlqMessage_ContainsDeadLetterHeaders()
    {
        // Arrange
        var testQueue = $"test.headers.{Guid.NewGuid():N}";
        var testExchange = $"test.headers.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.headers.key";

        // Declare exchange FIRST (before publishing)
        await _verificationChannel!.ExchangeDeclareAsync(
            testExchange,
            ExchangeType.Topic,
            durable: false,
            autoDelete: true);

        // Trigger publisher to declare DLX/DLQ
        await _publisher.PublishAsync(testExchange, "init", "{}");

        await _verificationChannel.QueueDeclareAsync(
            queue: testQueue,
            durable: false,
            exclusive: false,
            autoDelete: true,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "family-hub.dlx"
            });

        await _verificationChannel.QueueBindAsync(testQueue, testExchange, routingKey);

        var consumer = new AsyncEventingBasicConsumer(_verificationChannel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            await _verificationChannel.BasicNackAsync(args.DeliveryTag, false, false);
        };

        await _verificationChannel.BasicConsumeAsync(testQueue, autoAck: false, consumer);

        // Act
        await _publisher.PublishAsync(testExchange, routingKey, """{"test": "headers"}""");
        await Task.Delay(2000);

        // Assert
        var dlqMessage = await _verificationChannel.BasicGetAsync("family-hub.dlq", autoAck: false);
        dlqMessage.Should().NotBeNull();

        var headers = dlqMessage.BasicProperties.Headers;
        headers.Should().NotBeNull("dead-lettered message should have headers");

        // x-death header contains routing information
        headers.ContainsKey("x-death").Should().BeTrue(
            "dead-lettered message should contain x-death header with routing info");

        // Acknowledge after inspection
        await _verificationChannel.BasicAckAsync(dlqMessage.DeliveryTag, false);
    }

    #endregion

    #region Multiple Messages to DLQ Tests

    /// <summary>
    /// Tests that multiple rejected messages all appear in DLQ.
    /// </summary>
    [Fact]
    public async Task MultipleRejectedMessages_AllAppearInDlq()
    {
        // Arrange
        var testQueue = $"test.multi.{Guid.NewGuid():N}";
        var testExchange = $"test.multi.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.multi.key";
        const int messageCount = 5;

        // Declare exchange FIRST (before publishing)
        await _verificationChannel!.ExchangeDeclareAsync(
            testExchange,
            ExchangeType.Topic,
            durable: false,
            autoDelete: true);

        // Trigger publisher to declare DLX/DLQ
        await _publisher.PublishAsync(testExchange, "init", "{}");

        await _verificationChannel.QueueDeclareAsync(
            queue: testQueue,
            durable: false,
            exclusive: false,
            autoDelete: true,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "family-hub.dlx"
            });

        await _verificationChannel.QueueBindAsync(testQueue, testExchange, routingKey);

        var consumer = new AsyncEventingBasicConsumer(_verificationChannel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            await _verificationChannel.BasicNackAsync(args.DeliveryTag, false, false);
        };

        await _verificationChannel.BasicConsumeAsync(testQueue, autoAck: false, consumer);

        // Act - Publish multiple messages
        for (var i = 0; i < messageCount; i++)
        {
            await _publisher.PublishAsync(testExchange, routingKey, $$$"""{"index": {{{i}}}}""");
        }

        // Wait for all messages to be processed
        await Task.Delay(3000);

        // Assert - All messages should appear in DLQ
        var receivedCount = 0;
        BasicGetResult? result;
        do
        {
            result = await _verificationChannel.BasicGetAsync("family-hub.dlq", autoAck: true);
            if (result != null)
            {
                receivedCount++;
            }
        } while (result != null);

        receivedCount.Should().BeGreaterThanOrEqualTo(messageCount,
            $"all {messageCount} rejected messages should appear in DLQ");
    }

    #endregion

    #region Message Reprocessing Tests

    /// <summary>
    /// Tests that a message from DLQ can be republished for reprocessing.
    /// </summary>
    [Fact]
    public async Task DlqMessage_CanBeRepublished()
    {
        // Arrange
        var testQueue = $"test.republish.{Guid.NewGuid():N}";
        var testExchange = $"test.republish.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.republish.key";
        var originalMessage = """{"republish": "test"}""";

        // Declare exchange FIRST (before publishing)
        await _verificationChannel!.ExchangeDeclareAsync(
            testExchange,
            ExchangeType.Topic,
            durable: false,
            autoDelete: true);

        // Trigger publisher to declare DLX/DLQ
        await _publisher.PublishAsync(testExchange, "init", "{}");

        await _verificationChannel.QueueDeclareAsync(
            queue: testQueue,
            durable: false,
            exclusive: false,
            autoDelete: true,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "family-hub.dlx"
            });

        await _verificationChannel.QueueBindAsync(testQueue, testExchange, routingKey);

        // First consumer rejects
        var consumer = new AsyncEventingBasicConsumer(_verificationChannel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            await _verificationChannel.BasicNackAsync(args.DeliveryTag, false, false);
        };

        await _verificationChannel.BasicConsumeAsync(testQueue, autoAck: false, consumer);

        // Publish and wait for rejection
        await _publisher.PublishAsync(testExchange, routingKey, originalMessage);
        await Task.Delay(2000);

        // Get message from DLQ
        var dlqMessage = await _verificationChannel.BasicGetAsync("family-hub.dlq", autoAck: true);
        dlqMessage.Should().NotBeNull();

        // Act - Republish the message
        var reprocessQueue = $"test.reprocess.{Guid.NewGuid():N}";
        await _verificationChannel.QueueDeclareAsync(
            queue: reprocessQueue,
            durable: false,
            exclusive: true,
            autoDelete: true);
        await _verificationChannel.QueueBindAsync(reprocessQueue, testExchange, routingKey);

        await _verificationChannel.BasicPublishAsync(
            exchange: testExchange,
            routingKey: routingKey,
            body: dlqMessage.Body);

        await Task.Delay(500);

        // Assert - Message should be in reprocess queue
        var reprocessedMessage = await _verificationChannel.BasicGetAsync(reprocessQueue, autoAck: true);
        reprocessedMessage.Should().NotBeNull("republished message should be deliverable");

        var body = Encoding.UTF8.GetString(reprocessedMessage.Body.ToArray());
        body.Should().Contain("republish");
    }

    #endregion

    #region Helper Methods

    private async Task<bool> ExchangeExistsAsync(string exchangeName)
    {
        try
        {
            // Passive declare throws if exchange doesn't exist
            await _verificationChannel!.ExchangeDeclarePassiveAsync(exchangeName);
            return true;
        }
        catch (OperationInterruptedException)
        {
            // Exchange doesn't exist - need to reconnect channel
            _verificationChannel = await _verificationConnection!.CreateChannelAsync();
            return false;
        }
    }

    private async Task<bool> QueueExistsAsync(string queueName)
    {
        try
        {
            // Passive declare throws if queue doesn't exist
            await _verificationChannel!.QueueDeclarePassiveAsync(queueName);
            return true;
        }
        catch (OperationInterruptedException)
        {
            // Queue doesn't exist - need to reconnect channel
            _verificationChannel = await _verificationConnection!.CreateChannelAsync();
            return false;
        }
    }

    private RabbitMqSettings CreateTestSettings() => new()
    {
        Host = fixture.Host,
        Port = fixture.Port,
        Username = fixture.Username,
        Password = fixture.Password,
        DefaultExchange = "family-hub.events",
        DeadLetterExchange = "family-hub.dlx",
        DeadLetterQueue = "family-hub.dlq",
        MaxRetryAttempts = 3,
        RetryBaseDelay = TimeSpan.FromMilliseconds(100),
        ConnectionTimeout = TimeSpan.FromSeconds(10),
        EnablePublisherConfirms = true
    };

    #endregion
}
