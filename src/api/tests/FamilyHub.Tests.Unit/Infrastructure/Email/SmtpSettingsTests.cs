using FamilyHub.Infrastructure.Email;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.Infrastructure.Email;

/// <summary>
/// Unit tests for SmtpSettings.
/// Verifies default values and validation logic.
/// </summary>
public sealed class SmtpSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultHost_IsLocalhost()
    {
        // Arrange & Act
        var settings = new SmtpSettings();

        // Assert
        settings.Host.Should().Be("localhost");
    }

    [Fact]
    public void DefaultPort_Is587()
    {
        // Arrange & Act
        var settings = new SmtpSettings();

        // Assert
        settings.Port.Should().Be(587);
    }

    [Fact]
    public void DefaultUseTls_IsTrue()
    {
        // Arrange & Act
        var settings = new SmtpSettings();

        // Assert
        settings.UseTls.Should().BeTrue();
    }

    [Fact]
    public void DefaultUsername_IsNull()
    {
        // Arrange & Act
        var settings = new SmtpSettings();

        // Assert
        settings.Username.Should().BeNull();
    }

    [Fact]
    public void DefaultPassword_IsNull()
    {
        // Arrange & Act
        var settings = new SmtpSettings();

        // Assert
        settings.Password.Should().BeNull();
    }

    [Fact]
    public void DefaultFromAddress_IsNoReplyAtFamilyHub()
    {
        // Arrange & Act
        var settings = new SmtpSettings();

        // Assert
        settings.FromAddress.Should().Be("no-reply@familyhub.com");
    }

    [Fact]
    public void DefaultFromDisplayName_IsFamilyHub()
    {
        // Arrange & Act
        var settings = new SmtpSettings();

        // Assert
        settings.FromDisplayName.Should().Be("Family Hub");
    }

    [Fact]
    public void DefaultMaxRetryAttempts_Is3()
    {
        // Arrange & Act
        var settings = new SmtpSettings();

        // Assert
        settings.MaxRetryAttempts.Should().Be(3);
    }

    [Fact]
    public void DefaultRetryBaseDelay_Is1Second()
    {
        // Arrange & Act
        var settings = new SmtpSettings();

        // Assert
        settings.RetryBaseDelay.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void DefaultConnectionTimeout_Is30Seconds()
    {
        // Arrange & Act
        var settings = new SmtpSettings();

        // Assert
        settings.ConnectionTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void SectionName_IsSmtp()
    {
        // Assert
        SmtpSettings.SectionName.Should().Be("Smtp");
    }

    #endregion

    #region IsValid Tests

    [Fact]
    public void IsValid_WithDefaultSettings_ReturnsTrue()
    {
        // Arrange
        var settings = new SmtpSettings();

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyHost_ReturnsFalse()
    {
        // Arrange
        var settings = new SmtpSettings { Host = string.Empty };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceHost_ReturnsFalse()
    {
        // Arrange
        var settings = new SmtpSettings { Host = "   " };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNullHost_ReturnsFalse()
    {
        // Arrange
        var settings = new SmtpSettings { Host = null! };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithZeroPort_ReturnsFalse()
    {
        // Arrange
        var settings = new SmtpSettings { Port = 0 };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNegativePort_ReturnsFalse()
    {
        // Arrange
        var settings = new SmtpSettings { Port = -1 };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithPort65536_ReturnsFalse()
    {
        // Arrange - Port must be <= 65535
        var settings = new SmtpSettings { Port = 65536 };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithPort65535_ReturnsTrue()
    {
        // Arrange - Maximum valid port
        var settings = new SmtpSettings { Port = 65535 };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithPort1_ReturnsTrue()
    {
        // Arrange - Minimum valid port
        var settings = new SmtpSettings { Port = 1 };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyFromAddress_ReturnsFalse()
    {
        // Arrange
        var settings = new SmtpSettings { FromAddress = string.Empty };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceFromAddress_ReturnsFalse()
    {
        // Arrange
        var settings = new SmtpSettings { FromAddress = "   " };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNullFromAddress_ReturnsFalse()
    {
        // Arrange
        var settings = new SmtpSettings { FromAddress = null! };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithCustomValidSettings_ReturnsTrue()
    {
        // Arrange
        var settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 465,
            UseTls = true,
            Username = "admin@example.com",
            Password = "secret",
            FromAddress = "noreply@example.com",
            FromDisplayName = "Example App",
            MaxRetryAttempts = 5,
            RetryBaseDelay = TimeSpan.FromSeconds(2),
            ConnectionTimeout = TimeSpan.FromSeconds(60)
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithNullCredentials_ReturnsTrue()
    {
        // Arrange - Credentials are optional (authentication not required)
        var settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 25,
            Username = null,
            Password = null,
            FromAddress = "test@example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithUseTlsFalse_ReturnsTrue()
    {
        // Arrange - TLS is optional
        var settings = new SmtpSettings
        {
            Host = "localhost",
            Port = 1025,
            UseTls = false,
            FromAddress = "test@localhost"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData(25)]   // Standard SMTP
    [InlineData(587)]  // Submission (STARTTLS)
    [InlineData(465)]  // SMTPS (SSL/TLS)
    [InlineData(2525)] // Alternative port
    public void IsValid_WithCommonSmtpPorts_ReturnsTrue(int port)
    {
        // Arrange
        var settings = new SmtpSettings { Port = port };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithLongHost_ReturnsTrue()
    {
        // Arrange - Very long but valid hostname
        var settings = new SmtpSettings
        {
            Host = "very.long.subdomain.example.com.with.many.parts.example.org"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithIpAddressHost_ReturnsTrue()
    {
        // Arrange - IP address as host
        var settings = new SmtpSettings { Host = "192.168.1.100" };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    #endregion
}
