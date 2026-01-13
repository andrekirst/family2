using System.Reflection;
using System.Text;
using FamilyHub.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FamilyHub.Tests.Unit.Infrastructure.Messaging;

/// <summary>
/// Unit tests for RabbitMqHealthCheck.
/// Tests private helper methods using reflection and constructor behavior.
/// Integration tests with real RabbitMQ connectivity are in FamilyHub.Tests.Integration.
/// </summary>
public sealed class RabbitMqHealthCheckUnitTests
{
    private readonly ILogger<RabbitMqHealthCheck> _logger = Substitute.For<ILogger<RabbitMqHealthCheck>>();
    private readonly IOptions<RabbitMqSettings> _settingsOptions = Options.Create(new RabbitMqSettings());

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var healthCheck = new RabbitMqHealthCheck(_logger, _settingsOptions);

        // Assert
        healthCheck.Should().NotBeNull();
        healthCheck.Should().BeAssignableTo<IHealthCheck>();
    }

    [Fact]
    public void Constructor_WithCustomSettings_UsesProvidedSettings()
    {
        // Arrange
        var customSettings = new RabbitMqSettings
        {
            Host = "custom-host",
            Port = 5673
        };
        var customOptions = Options.Create(customSettings);

        // Act
        var healthCheck = new RabbitMqHealthCheck(_logger, customOptions);

        // Assert
        healthCheck.Should().NotBeNull();
    }

    #endregion

    #region ConvertToString Tests (via reflection)

    [Fact]
    public void ConvertToString_WithByteArray_ConvertsToUtf8String()
    {
        // Arrange
        var expected = "RabbitMQ";
        var byteArray = Encoding.UTF8.GetBytes(expected);

        // Act
        var result = InvokeConvertToString(byteArray);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertToString_WithString_ReturnsString()
    {
        // Arrange
        var expected = "TestString";

        // Act
        var result = InvokeConvertToString(expected);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertToString_WithOtherObject_ReturnsToString()
    {
        // Arrange
        var intValue = 12345;

        // Act
        var result = InvokeConvertToString(intValue);

        // Assert
        result.Should().Be("12345");
    }

    [Fact]
    public void ConvertToString_WithNull_ReturnsUnknown()
    {
        // Arrange
        object? nullValue = null;

        // Act
        var result = InvokeConvertToString(nullValue);

        // Assert
        result.Should().Be("Unknown");
    }

    #endregion

    #region ExtractServerProperties Tests (via reflection)

    [Fact]
    public void ExtractServerProperties_WithBothValues_ExtractsCorrectly()
    {
        // Arrange
        var serverProperties = new Dictionary<string, object?>
        {
            ["product"] = Encoding.UTF8.GetBytes("RabbitMQ"),
            ["version"] = Encoding.UTF8.GetBytes("3.13.0")
        };

        // Act
        var (product, version) = InvokeExtractServerProperties(serverProperties);

        // Assert
        product.Should().Be("RabbitMQ");
        version.Should().Be("3.13.0");
    }

    [Fact]
    public void ExtractServerProperties_WithMissingProduct_ReturnsUnknown()
    {
        // Arrange
        var serverProperties = new Dictionary<string, object?>
        {
            ["version"] = Encoding.UTF8.GetBytes("3.13.0")
        };

        // Act
        var (product, version) = InvokeExtractServerProperties(serverProperties);

        // Assert
        product.Should().Be("Unknown");
        version.Should().Be("3.13.0");
    }

    [Fact]
    public void ExtractServerProperties_WithMissingVersion_ReturnsUnknown()
    {
        // Arrange
        var serverProperties = new Dictionary<string, object?>
        {
            ["product"] = Encoding.UTF8.GetBytes("RabbitMQ")
        };

        // Act
        var (product, version) = InvokeExtractServerProperties(serverProperties);

        // Assert
        product.Should().Be("RabbitMQ");
        version.Should().Be("Unknown");
    }

    [Fact]
    public void ExtractServerProperties_WithEmptyDictionary_ReturnsBothUnknown()
    {
        // Arrange
        var serverProperties = new Dictionary<string, object?>();

        // Act
        var (product, version) = InvokeExtractServerProperties(serverProperties);

        // Assert
        product.Should().Be("Unknown");
        version.Should().Be("Unknown");
    }

    [Fact]
    public void ExtractServerProperties_WithStringValues_ExtractsCorrectly()
    {
        // Arrange - Server properties can also be strings
        var serverProperties = new Dictionary<string, object?>
        {
            ["product"] = "RabbitMQ",
            ["version"] = "3.13.0"
        };

        // Act
        var (product, version) = InvokeExtractServerProperties(serverProperties);

        // Assert
        product.Should().Be("RabbitMQ");
        version.Should().Be("3.13.0");
    }

    #endregion

    #region Helper Methods for Reflection

    private static string InvokeConvertToString(object? value)
    {
        var method = typeof(RabbitMqHealthCheck).GetMethod(
            "ConvertToString",
            BindingFlags.NonPublic | BindingFlags.Static);

        method.Should().NotBeNull("ConvertToString method should exist");

        return (string)method!.Invoke(null, [value])!;
    }

    private static (string Product, string Version) InvokeExtractServerProperties(
        IDictionary<string, object?> serverProperties)
    {
        var method = typeof(RabbitMqHealthCheck).GetMethod(
            "ExtractServerProperties",
            BindingFlags.NonPublic | BindingFlags.Static);

        method.Should().NotBeNull("ExtractServerProperties method should exist");

        var result = method!.Invoke(null, [serverProperties]);
        return ((string, string))result!;
    }

    #endregion
}
