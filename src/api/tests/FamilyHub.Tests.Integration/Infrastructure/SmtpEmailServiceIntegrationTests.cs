using FamilyHub.Infrastructure.Email;
using FamilyHub.Infrastructure.Email.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for SmtpEmailService.
/// Tests email sending, connection management, and error handling with a real/mock SMTP server.
/// NOTE: These tests require MailHog running on localhost:1025 or will be skipped.
/// </summary>
public sealed class SmtpEmailServiceIntegrationTests : IAsyncLifetime
{
    private readonly ILogger<SmtpEmailService> _logger = Substitute.For<ILogger<SmtpEmailService>>();
    private SmtpEmailService? _smtpService;
    private bool _smtpAvailable;

    public async Task InitializeAsync()
    {
        // Check if SMTP server (MailHog) is available
        _smtpAvailable = await IsSmtpServerAvailableAsync();

        if (_smtpAvailable)
        {
            var settings = new SmtpSettings
            {
                Host = "localhost",
                Port = 1025,
                UseTls = false,
                FromAddress = "test@familyhub.local",
                FromDisplayName = "FamilyHub Test",
                MaxRetryAttempts = 3,
                RetryBaseDelay = TimeSpan.FromMilliseconds(100),
                ConnectionTimeout = TimeSpan.FromSeconds(5)
            };

            _smtpService = new SmtpEmailService(Options.Create(settings), _logger);
        }
    }

    public async Task DisposeAsync()
    {
        if (_smtpService != null)
        {
            await _smtpService.DisposeAsync();
        }
    }

    private static async Task<bool> IsSmtpServerAvailableAsync()
    {
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync("localhost", 1025);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #region SendEmailAsync Tests

    [Fact]
    public async Task SendEmailAsync_WithValidMessage_SendsSuccessfully()
    {
        // Skip if SMTP not available
        if (!_smtpAvailable)
        {
            // Use Skip.If pattern for conditional test skipping
            return;
        }

        // Arrange
        var emailMessage = new EmailMessage
        {
            To = "recipient@example.com",
            ToName = "Test Recipient",
            Subject = "Integration Test Email",
            HtmlBody = "<html><body><h1>Test Email</h1><p>This is a test email from integration tests.</p></body></html>",
            TextBody = "Test Email\nThis is a test email from integration tests."
        };

        // Act
        var result = await _smtpService!.SendEmailAsync(emailMessage);

        // Assert
        result.Should().BeTrue("email should be sent successfully to MailHog");
    }

    [Fact]
    public async Task SendEmailAsync_WithHtmlOnly_SendsSuccessfully()
    {
        if (!_smtpAvailable)
        {
            return;
        }

        // Arrange
        var emailMessage = new EmailMessage
        {
            To = "htmlonly@example.com",
            ToName = "HTML Only Recipient",
            Subject = "HTML Only Email",
            HtmlBody = "<html><body><p>HTML only content</p></body></html>"
        };

        // Act
        var result = await _smtpService!.SendEmailAsync(emailMessage);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_WithSpecialCharacters_SendsSuccessfully()
    {
        if (!_smtpAvailable)
        {
            return;
        }

        // Arrange
        var emailMessage = new EmailMessage
        {
            To = "special@example.com",
            ToName = "Test Üser with Späcial Chärs",
            Subject = "Tëst Sübject with émojis 🎉",
            HtmlBody = "<html><body><p>Content with special characters: äöü ÄÖÜ ß €</p></body></html>"
        };

        // Act
        var result = await _smtpService!.SendEmailAsync(emailMessage);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_WithLongSubject_SendsSuccessfully()
    {
        if (!_smtpAvailable)
        {
            return;
        }

        // Arrange
        var longSubject = new string('A', 200); // 200 character subject
        var emailMessage = new EmailMessage
        {
            To = "longsubject@example.com",
            ToName = "Long Subject Test",
            Subject = longSubject,
            HtmlBody = "<html><body><p>Email with long subject</p></body></html>"
        };

        // Act
        var result = await _smtpService!.SendEmailAsync(emailMessage);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_WithLargeHtmlBody_SendsSuccessfully()
    {
        if (!_smtpAvailable)
        {
            return;
        }

        // Arrange
        var largeBody = "<html><body>" + new string('X', 50000) + "</body></html>"; // 50KB body
        var emailMessage = new EmailMessage
        {
            To = "largebody@example.com",
            ToName = "Large Body Test",
            Subject = "Large Email Body Test",
            HtmlBody = largeBody
        };

        // Act
        var result = await _smtpService!.SendEmailAsync(emailMessage);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SendEmailAsync_WithInvalidSmtpServer_ReturnsFalse()
    {
        // Arrange - Create service with invalid SMTP settings
        var invalidSettings = new SmtpSettings
        {
            Host = "invalid.smtp.server.local",
            Port = 9999,
            UseTls = false,
            FromAddress = "test@example.com",
            FromDisplayName = "Test",
            MaxRetryAttempts = 1,
            RetryBaseDelay = TimeSpan.FromMilliseconds(10),
            ConnectionTimeout = TimeSpan.FromSeconds(2)
        };

        await using var invalidService = new SmtpEmailService(Options.Create(invalidSettings), _logger);

        var emailMessage = new EmailMessage
        {
            To = "test@example.com",
            ToName = "Test",
            Subject = "Test",
            HtmlBody = "<html><body>Test</body></html>"
        };

        // Act
        var result = await invalidService.SendEmailAsync(emailMessage);

        // Assert
        result.Should().BeFalse("connection to invalid SMTP server should fail");
    }

    #endregion

    #region Multiple Email Tests

    [Fact]
    public async Task SendEmailAsync_MultipleEmails_AllSendSuccessfully()
    {
        if (!_smtpAvailable)
        {
            return;
        }

        // Arrange
        var emailCount = 5;
        var results = new List<bool>();

        // Act
        for (var i = 0; i < emailCount; i++)
        {
            var emailMessage = new EmailMessage
            {
                To = $"multiple{i}@example.com",
                ToName = $"Recipient {i}",
                Subject = $"Multiple Email Test {i}",
                HtmlBody = $"<html><body><p>Email number {i}</p></body></html>"
            };

            var result = await _smtpService!.SendEmailAsync(emailMessage);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(emailCount);
        results.Should().OnlyContain(r => r, "all emails should be sent successfully");
    }

    [Fact]
    public async Task SendEmailAsync_ConcurrentEmails_AllSendSuccessfully()
    {
        if (!_smtpAvailable)
        {
            return;
        }

        // Arrange
        var emailCount = 3;
        var tasks = new List<Task<bool>>();

        // Act
        for (var i = 0; i < emailCount; i++)
        {
            var index = i; // Capture for closure
            var emailMessage = new EmailMessage
            {
                To = $"concurrent{index}@example.com",
                ToName = $"Concurrent {index}",
                Subject = $"Concurrent Email Test {index}",
                HtmlBody = $"<html><body><p>Concurrent email {index}</p></body></html>"
            };

            tasks.Add(_smtpService!.SendEmailAsync(emailMessage));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(emailCount);
        results.Should().OnlyContain(r => r, "all concurrent emails should be sent successfully");
    }

    #endregion

    #region Connection Management Tests

    [Fact]
    public async Task SendEmailAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        if (!_smtpAvailable)
        {
            return;
        }

        // Arrange
        var settings = new SmtpSettings
        {
            Host = "localhost",
            Port = 1025,
            UseTls = false,
            FromAddress = "test@example.com",
            FromDisplayName = "Test",
            MaxRetryAttempts = 1,
            RetryBaseDelay = TimeSpan.FromMilliseconds(10)
        };

        var service = new SmtpEmailService(Options.Create(settings), _logger);
        await service.DisposeAsync();

        var emailMessage = new EmailMessage
        {
            To = "test@example.com",
            ToName = "Test",
            Subject = "Test",
            HtmlBody = "<html><body>Test</body></html>"
        };

        // Act & Assert
        await FluentActions.Awaiting(() => service.SendEmailAsync(emailMessage))
            .Should().ThrowAsync<ObjectDisposedException>();
    }

    #endregion
}
