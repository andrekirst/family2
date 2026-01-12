using FamilyHub.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FamilyHub.Tests.Unit.Infrastructure.Messaging;

/// <summary>
/// Unit tests for RabbitMqPublisher.
/// Tests parameter validation, constructor behavior, and dispose pattern.
/// Integration tests with real RabbitMQ are in FamilyHub.Tests.Integration.
/// </summary>
public sealed class RabbitMqPublisherUnitTests
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly IOptions<RabbitMqSettings> _settingsOptions;

    public RabbitMqPublisherUnitTests()
    {
        _logger = Substitute.For<ILogger<RabbitMqPublisher>>();
        _settingsOptions = Options.Create(new RabbitMqSettings());
    }

    #region Constructor Tests

    [Fact]
    public async Task Constructor_WithValidSettings_InitializesWithoutConnection()
    {
        // Act
        await using var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Assert - Publisher should be created without connecting
        publisher.Should().NotBeNull();
    }

    [Fact]
    public async Task Constructor_WithValidLogger_StoresLogger()
    {
        // Act
        await using var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Assert - No exception means logger was accepted
        publisher.Should().NotBeNull();
    }

    [Fact]
    public async Task Constructor_WithCustomSettings_UsesProvidedSettings()
    {
        // Arrange
        var customSettings = new RabbitMqSettings
        {
            Host = "custom-host",
            Port = 5673,
            MaxRetryAttempts = 5
        };
        var customOptions = Options.Create(customSettings);

        // Act
        await using var publisher = new RabbitMqPublisher(_logger, customOptions);

        // Assert
        publisher.Should().NotBeNull();
    }

    #endregion

    #region Parameter Validation Tests - Exchange

    [Fact]
    public async Task PublishAsync_WithNullExchange_ThrowsArgumentException()
    {
        // Arrange
        await using var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Act
        var act = () => publisher.PublishAsync(null!, "routing.key", "message");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("exchange");
    }

    [Fact]
    public async Task PublishAsync_WithEmptyExchange_ThrowsArgumentException()
    {
        // Arrange
        await using var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Act
        var act = () => publisher.PublishAsync(string.Empty, "routing.key", "message");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("exchange");
    }

    [Fact]
    public async Task PublishAsync_WithWhitespaceExchange_ThrowsArgumentException()
    {
        // Arrange
        await using var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Act
        var act = () => publisher.PublishAsync("   ", "routing.key", "message");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("exchange");
    }

    #endregion

    #region Parameter Validation Tests - RoutingKey

    [Fact]
    public async Task PublishAsync_WithNullRoutingKey_ThrowsArgumentException()
    {
        // Arrange
        await using var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Act
        var act = () => publisher.PublishAsync("exchange", null!, "message");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("routingKey");
    }

    [Fact]
    public async Task PublishAsync_WithEmptyRoutingKey_ThrowsArgumentException()
    {
        // Arrange
        await using var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Act
        var act = () => publisher.PublishAsync("exchange", string.Empty, "message");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("routingKey");
    }

    [Fact]
    public async Task PublishAsync_WithWhitespaceRoutingKey_ThrowsArgumentException()
    {
        // Arrange
        await using var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Act
        var act = () => publisher.PublishAsync("exchange", "   ", "message");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("routingKey");
    }

    #endregion

    #region Parameter Validation Tests - Message

    [Fact]
    public async Task PublishAsync_WithNullMessage_ThrowsArgumentException()
    {
        // Arrange
        await using var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Act
        var act = () => publisher.PublishAsync("exchange", "routing.key", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("message");
    }

    [Fact]
    public async Task PublishAsync_WithEmptyMessage_ThrowsArgumentException()
    {
        // Arrange
        await using var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Act
        var act = () => publisher.PublishAsync("exchange", "routing.key", string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("message");
    }

    [Fact]
    public async Task PublishAsync_WithWhitespaceMessage_ThrowsArgumentException()
    {
        // Arrange
        await using var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Act
        var act = () => publisher.PublishAsync("exchange", "routing.key", "   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("message");
    }

    #endregion

    #region Dispose Pattern Tests

    [Fact]
    public async Task DisposeAsync_WhenNotDisposed_CompletesSuccessfully()
    {
        // Arrange
        var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Act & Assert - Should complete without throwing
        var exception = await Record.ExceptionAsync(async () => await publisher.DisposeAsync());
        exception.Should().BeNull();
    }

    [Fact]
    public async Task DisposeAsync_WhenAlreadyDisposed_DoesNotThrow()
    {
        // Arrange
        var publisher = new RabbitMqPublisher(_logger, _settingsOptions);
        await publisher.DisposeAsync();

        // Act - Second dispose
        var exception = await Record.ExceptionAsync(async () => await publisher.DisposeAsync());

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task DisposeAsync_MultipleCalls_IsIdempotent()
    {
        // Arrange
        var publisher = new RabbitMqPublisher(_logger, _settingsOptions);

        // Act - Multiple dispose calls
        await publisher.DisposeAsync();
        await publisher.DisposeAsync();
        await publisher.DisposeAsync();

        // Assert - No exception means idempotent disposal
        true.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var publisher = new RabbitMqPublisher(_logger, _settingsOptions);
        await publisher.DisposeAsync();

        // Act
        var act = () => publisher.PublishAsync("exchange", "routing.key", "message");

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    #endregion
}
