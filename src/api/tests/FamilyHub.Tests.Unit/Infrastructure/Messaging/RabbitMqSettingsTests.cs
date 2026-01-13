using FamilyHub.Infrastructure.Messaging;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.Infrastructure.Messaging;

/// <summary>
/// Unit tests for RabbitMqSettings.
/// Verifies default values and validation logic.
/// </summary>
public sealed class RabbitMqSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultHost_IsLocalhost()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.Host.Should().Be("localhost");
    }

    [Fact]
    public void DefaultPort_Is5672()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.Port.Should().Be(5672);
    }

    [Fact]
    public void DefaultUsername_IsGuest()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.Username.Should().Be("guest");
    }

    [Fact]
    public void DefaultPassword_IsGuest()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.Password.Should().Be("guest");
    }

    [Fact]
    public void DefaultVirtualHost_IsSlash()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.VirtualHost.Should().Be("/");
    }

    [Fact]
    public void DefaultMaxRetryAttempts_Is3()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.MaxRetryAttempts.Should().Be(3);
    }

    [Fact]
    public void DefaultEnablePublisherConfirms_IsTrue()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.EnablePublisherConfirms.Should().BeTrue();
    }

    [Fact]
    public void DefaultClientProvidedName_IsFamilyHubApi()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.ClientProvidedName.Should().Be("FamilyHub.Api");
    }

    [Fact]
    public void DefaultExchange_IsFamilyHubEvents()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.DefaultExchange.Should().Be("family-hub.events");
    }

    [Fact]
    public void DefaultDeadLetterExchange_IsFamilyHubDlx()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.DeadLetterExchange.Should().Be("family-hub.dlx");
    }

    [Fact]
    public void DefaultDeadLetterQueue_IsFamilyHubDlq()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.DeadLetterQueue.Should().Be("family-hub.dlq");
    }

    [Fact]
    public void DefaultRetryBaseDelay_Is1Second()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.RetryBaseDelay.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void DefaultRetryMaxDelay_Is30Seconds()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.RetryMaxDelay.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void DefaultConnectionTimeout_Is30Seconds()
    {
        // Arrange & Act
        var settings = new RabbitMqSettings();

        // Assert
        settings.ConnectionTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void SectionName_IsRabbitMQ()
    {
        // Assert
        RabbitMqSettings.SectionName.Should().Be("RabbitMQ");
    }

    #endregion

    #region IsValid Tests

    [Fact]
    public void IsValid_WithDefaultSettings_ReturnsTrue()
    {
        // Arrange
        var settings = new RabbitMqSettings();

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyHost_ReturnsFalse()
    {
        // Arrange
        var settings = new RabbitMqSettings { Host = string.Empty };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceHost_ReturnsFalse()
    {
        // Arrange
        var settings = new RabbitMqSettings { Host = "   " };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithZeroPort_ReturnsFalse()
    {
        // Arrange
        var settings = new RabbitMqSettings { Port = 0 };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNegativePort_ReturnsFalse()
    {
        // Arrange
        var settings = new RabbitMqSettings { Port = -1 };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyUsername_ReturnsFalse()
    {
        // Arrange
        var settings = new RabbitMqSettings { Username = string.Empty };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyPassword_ReturnsFalse()
    {
        // Arrange
        var settings = new RabbitMqSettings { Password = string.Empty };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNegativeRetryAttempts_ReturnsFalse()
    {
        // Arrange
        var settings = new RabbitMqSettings { MaxRetryAttempts = -1 };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithZeroRetryAttempts_ReturnsTrue()
    {
        // Arrange - Zero retries is valid (means no retries)
        var settings = new RabbitMqSettings { MaxRetryAttempts = 0 };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithCustomValidSettings_ReturnsTrue()
    {
        // Arrange
        var settings = new RabbitMqSettings
        {
            Host = "rabbitmq.example.com",
            Port = 5673,
            Username = "admin",
            Password = "secret",
            VirtualHost = "/production",
            MaxRetryAttempts = 5
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    #endregion
}
