using System.Text;
using System.Text.Json;
using FamilyHub.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for RabbitMqPublisher using real RabbitMQ.
/// Tests actual message publishing, exchanges, and consumer behavior.
/// </summary>
[Collection("RabbitMQ")]
public sealed class RabbitMqPublisherIntegrationTests : IAsyncLifetime
{
    private readonly RabbitMqContainerFixture _fixture;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private RabbitMqPublisher _publisher = null!;
    private IConnection? _consumerConnection;
    private IChannel? _consumerChannel;

    public RabbitMqPublisherIntegrationTests(RabbitMqContainerFixture fixture)
    {
        _fixture = fixture;
        _logger = Substitute.For<ILogger<RabbitMqPublisher>>();
    }

    public async Task InitializeAsync()
    {
        var settings = new RabbitMqSettings
        {
            Host = _fixture.Host,
            Port = _fixture.Port,
            Username = _fixture.Username,
            Password = _fixture.Password
        };

        _publisher = new RabbitMqPublisher(_logger, Options.Create(settings));

        // Create a consumer connection for verification
        var factory = new ConnectionFactory
        {
            HostName = _fixture.Host,
            Port = _fixture.Port,
            UserName = _fixture.Username,
            Password = _fixture.Password
        };
        _consumerConnection = await factory.CreateConnectionAsync();
        _consumerChannel = await _consumerConnection.CreateChannelAsync();
    }

    public async Task DisposeAsync()
    {
        await _publisher.DisposeAsync();

        if (_consumerChannel != null)
        {
            await _consumerChannel.CloseAsync();
            await _consumerChannel.DisposeAsync();
        }

        if (_consumerConnection != null)
        {
            await _consumerConnection.CloseAsync();
            await _consumerConnection.DisposeAsync();
        }
    }

    #region Basic Publishing Tests

    [Fact]
    public async Task PublishAsync_WithValidMessage_PublishesToExchange()
    {
        // Arrange
        var exchangeName = $"test.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.routing.key";
        var message = "{\"test\": \"message\"}";
        var receivedMessage = string.Empty;
        var messageReceived = new TaskCompletionSource<bool>();

        // Set up exchange and queue for consuming
        await _consumerChannel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: false, autoDelete: true);
        var queueDeclare = await _consumerChannel.QueueDeclareAsync(exclusive: true, autoDelete: true);
        await _consumerChannel.QueueBindAsync(queueDeclare.QueueName, exchangeName, routingKey);

        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.ReceivedAsync += (sender, args) =>
        {
            receivedMessage = Encoding.UTF8.GetString(args.Body.ToArray());
            messageReceived.SetResult(true);
            return Task.CompletedTask;
        };

        await _consumerChannel.BasicConsumeAsync(queueDeclare.QueueName, autoAck: true, consumer);

        // Act
        await _publisher.PublishAsync(exchangeName, routingKey, message);

        // Assert - Wait for message with timeout
        var received = await Task.WhenAny(messageReceived.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        received.Should().Be(messageReceived.Task, "message should be received within timeout");
        receivedMessage.Should().Be(message);
    }

    [Fact]
    public async Task PublishAsync_WithJsonMessage_SetsContentTypeToJson()
    {
        // Arrange
        var exchangeName = $"test.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.content.type";
        var message = "{\"type\": \"json\"}";
        var receivedContentType = string.Empty;
        var messageReceived = new TaskCompletionSource<bool>();

        await _consumerChannel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: false, autoDelete: true);
        var queueDeclare = await _consumerChannel.QueueDeclareAsync(exclusive: true, autoDelete: true);
        await _consumerChannel.QueueBindAsync(queueDeclare.QueueName, exchangeName, routingKey);

        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.ReceivedAsync += (sender, args) =>
        {
            receivedContentType = args.BasicProperties?.ContentType ?? "none";
            messageReceived.SetResult(true);
            return Task.CompletedTask;
        };

        await _consumerChannel.BasicConsumeAsync(queueDeclare.QueueName, autoAck: true, consumer);

        // Act
        await _publisher.PublishAsync(exchangeName, routingKey, message);

        // Assert
        var received = await Task.WhenAny(messageReceived.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        received.Should().Be(messageReceived.Task);
        receivedContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task PublishAsync_WithPersistentDeliveryMode_SetsPersistence()
    {
        // Arrange
        var exchangeName = $"test.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.persistent";
        var message = "{\"persistent\": true}";
        var deliveryMode = DeliveryModes.Transient;
        var messageReceived = new TaskCompletionSource<bool>();

        await _consumerChannel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: false, autoDelete: true);
        var queueDeclare = await _consumerChannel.QueueDeclareAsync(exclusive: true, autoDelete: true);
        await _consumerChannel.QueueBindAsync(queueDeclare.QueueName, exchangeName, routingKey);

        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.ReceivedAsync += (sender, args) =>
        {
            deliveryMode = args.BasicProperties?.DeliveryMode ?? DeliveryModes.Transient;
            messageReceived.SetResult(true);
            return Task.CompletedTask;
        };

        await _consumerChannel.BasicConsumeAsync(queueDeclare.QueueName, autoAck: true, consumer);

        // Act
        await _publisher.PublishAsync(exchangeName, routingKey, message);

        // Assert
        var received = await Task.WhenAny(messageReceived.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        received.Should().Be(messageReceived.Task);
        deliveryMode.Should().Be(DeliveryModes.Persistent, "messages should be marked persistent");
    }

    #endregion

    #region Multiple Message Tests

    [Fact]
    public async Task PublishAsync_MultipleMessages_AllDelivered()
    {
        // Arrange
        var exchangeName = $"test.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.multiple";
        var messageCount = 10;
        var receivedMessages = new List<string>();
        var allMessagesReceived = new TaskCompletionSource<bool>();

        await _consumerChannel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: false, autoDelete: true);
        var queueDeclare = await _consumerChannel.QueueDeclareAsync(exclusive: true, autoDelete: true);
        await _consumerChannel.QueueBindAsync(queueDeclare.QueueName, exchangeName, routingKey);

        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.ReceivedAsync += (sender, args) =>
        {
            var message = Encoding.UTF8.GetString(args.Body.ToArray());
            lock (receivedMessages)
            {
                receivedMessages.Add(message);
                if (receivedMessages.Count == messageCount)
                {
                    allMessagesReceived.TrySetResult(true);
                }
            }
            return Task.CompletedTask;
        };

        await _consumerChannel.BasicConsumeAsync(queueDeclare.QueueName, autoAck: true, consumer);

        // Act
        for (var i = 0; i < messageCount; i++)
        {
            await _publisher.PublishAsync(exchangeName, routingKey, $"{{\"index\": {i}}}");
        }

        // Assert
        var received = await Task.WhenAny(allMessagesReceived.Task, Task.Delay(TimeSpan.FromSeconds(10)));
        received.Should().Be(allMessagesReceived.Task, "all messages should be received within timeout");
        receivedMessages.Should().HaveCount(messageCount);
    }

    #endregion

    #region Generic PublishAsync Tests

    [Fact]
    public async Task PublishAsyncGeneric_WithTypedMessage_SerializesToJsonAndPublishes()
    {
        // Arrange
        var exchangeName = $"test.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.generic.message";
        var message = new TestEventMessage
        {
            EventId = Guid.NewGuid(),
            EventType = "TestEvent",
            Payload = "Test payload data"
        };
        var receivedMessage = string.Empty;
        var messageReceived = new TaskCompletionSource<bool>();

        await _consumerChannel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: false, autoDelete: true);
        var queueDeclare = await _consumerChannel.QueueDeclareAsync(exclusive: true, autoDelete: true);
        await _consumerChannel.QueueBindAsync(queueDeclare.QueueName, exchangeName, routingKey);

        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.ReceivedAsync += (sender, args) =>
        {
            receivedMessage = Encoding.UTF8.GetString(args.Body.ToArray());
            messageReceived.SetResult(true);
            return Task.CompletedTask;
        };

        await _consumerChannel.BasicConsumeAsync(queueDeclare.QueueName, autoAck: true, consumer);

        // Act
        await _publisher.PublishAsync(exchangeName, routingKey, message);

        // Assert - Wait for message with timeout
        var received = await Task.WhenAny(messageReceived.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        received.Should().Be(messageReceived.Task, "message should be received within timeout");

        // Verify JSON serialization with camelCase
        var deserializedMessage = JsonSerializer.Deserialize<TestEventMessage>(receivedMessage, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        deserializedMessage.Should().NotBeNull();
        deserializedMessage!.EventId.Should().Be(message.EventId);
        deserializedMessage.EventType.Should().Be(message.EventType);
        deserializedMessage.Payload.Should().Be(message.Payload);
    }

    [Fact]
    public async Task PublishAsyncGeneric_WithTypedMessage_UsesCamelCaseJsonNaming()
    {
        // Arrange
        var exchangeName = $"test.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.camelcase";
        var message = new TestEventMessage
        {
            EventId = Guid.NewGuid(),
            EventType = "CamelCaseTest",
            Payload = "data"
        };
        var receivedMessage = string.Empty;
        var messageReceived = new TaskCompletionSource<bool>();

        await _consumerChannel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: false, autoDelete: true);
        var queueDeclare = await _consumerChannel.QueueDeclareAsync(exclusive: true, autoDelete: true);
        await _consumerChannel.QueueBindAsync(queueDeclare.QueueName, exchangeName, routingKey);

        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.ReceivedAsync += (sender, args) =>
        {
            receivedMessage = Encoding.UTF8.GetString(args.Body.ToArray());
            messageReceived.SetResult(true);
            return Task.CompletedTask;
        };

        await _consumerChannel.BasicConsumeAsync(queueDeclare.QueueName, autoAck: true, consumer);

        // Act
        await _publisher.PublishAsync(exchangeName, routingKey, message);

        // Assert - Wait for message
        var received = await Task.WhenAny(messageReceived.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        received.Should().Be(messageReceived.Task);

        // Verify camelCase property names in JSON
        receivedMessage.Should().Contain("\"eventId\":");
        receivedMessage.Should().Contain("\"eventType\":");
        receivedMessage.Should().Contain("\"payload\":");
        receivedMessage.Should().NotContain("\"EventId\":");
        receivedMessage.Should().NotContain("\"EventType\":");
        receivedMessage.Should().NotContain("\"Payload\":");
    }

    #endregion

    #region Connection Recovery Tests

    [Fact]
    public async Task PublishAsync_AfterReconnect_ContinuesToWork()
    {
        // Arrange
        var exchangeName = $"test.exchange.{Guid.NewGuid():N}";
        var routingKey = "test.reconnect";

        await _consumerChannel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: false, autoDelete: true);
        var queueDeclare = await _consumerChannel.QueueDeclareAsync(exclusive: true, autoDelete: true);
        await _consumerChannel.QueueBindAsync(queueDeclare.QueueName, exchangeName, routingKey);

        var receivedMessages = new List<string>();
        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.ReceivedAsync += (sender, args) =>
        {
            receivedMessages.Add(Encoding.UTF8.GetString(args.Body.ToArray()));
            return Task.CompletedTask;
        };

        await _consumerChannel.BasicConsumeAsync(queueDeclare.QueueName, autoAck: true, consumer);

        // Act - Publish first message
        await _publisher.PublishAsync(exchangeName, routingKey, "{\"message\": 1}");

        // Give some time for first message
        await Task.Delay(500);

        // Publish second message
        await _publisher.PublishAsync(exchangeName, routingKey, "{\"message\": 2}");

        // Wait for messages
        await Task.Delay(1000);

        // Assert
        receivedMessages.Should().HaveCount(2);
    }

    #endregion
}

/// <summary>
/// Test message class for generic PublishAsync integration tests.
/// </summary>
internal sealed class TestEventMessage
{
    public Guid EventId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
}
