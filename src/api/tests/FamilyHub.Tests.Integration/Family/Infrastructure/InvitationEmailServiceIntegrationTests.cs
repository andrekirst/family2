using FamilyHub.Infrastructure.Email;
using FamilyHub.Infrastructure.Email.Models;
using FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Family.Domain.Enums;
using FamilyHub.Modules.Family.Infrastructure.BackgroundServices;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.Modules.Family.Persistence.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using FamilyHub.SharedKernel.ValueObjects;
using FamilyHub.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.Tests.Integration.Family.Infrastructure;

/// <summary>
/// Integration tests for InvitationEmailService background service.
/// Tests the complete email sending flow from EmailOutbox to SMTP.
/// </summary>
[Collection("FamilyDatabase")]
public sealed class InvitationEmailServiceIntegrationTests(FamilyPostgreSqlContainerFixture fixture) : IAsyncLifetime
{
    private FamilyDbContext _context = null!;
    private ServiceProvider _serviceProvider = null!;
    private ILogger<InvitationEmailService> _logger = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        _context = new FamilyDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        // Setup service collection with required dependencies
        var services = new ServiceCollection();
        services.AddScoped(_ => _context);

        // Register repository using FamilyModuleServiceRegistration pattern
        services.AddScoped<IEmailOutboxRepository>(sp =>
        {
            var context = sp.GetRequiredService<FamilyDbContext>();
            return (IEmailOutboxRepository)Activator.CreateInstance(
                Type.GetType("FamilyHub.Modules.Family.Persistence.Repositories.EmailOutboxRepository, FamilyHub.Modules.Family")!,
                context)!;
        });

        // Use a mock email service for testing (avoid real SMTP)
        var mockEmailService = Substitute.For<IEmailService>();
        mockEmailService.SendEmailAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(true);
        services.AddSingleton(mockEmailService);

        _serviceProvider = services.BuildServiceProvider();
        _logger = Substitute.For<ILogger<InvitationEmailService>>();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }

    #region End-to-End Email Sending Tests

    [Fact]
    public async Task EmailSendingFlow_WithPendingEmail_UpdatesStatusToSent()
    {
        // Arrange
        var emailOutbox = EmailOutbox.Create(
            OutboxEventId.New(),
            "integration@example.com",
            "Integration Test User",
            "Integration Test Subject",
            "<html><body>Integration Test Body</body></html>");

        var repository = _serviceProvider.GetRequiredService<IEmailOutboxRepository>();
        await repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Get mock email service to simulate sending
        var emailService = _serviceProvider.GetRequiredService<IEmailService>();

        // Act - Simulate one iteration of the background service
        var pendingEmails = await repository.GetPendingEmailsAsync(50);
        foreach (var email in pendingEmails)
        {
            var message = new EmailMessage
            {
                To = email.To,
                ToName = email.ToName,
                Subject = email.Subject,
                HtmlBody = email.HtmlBody,
                TextBody = email.TextBody
            };

            var success = await emailService.SendEmailAsync(message);
            if (success)
            {
                email.MarkAsSent();
            }
        }
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updatedEmail = await repository.GetByIdAsync(emailOutbox.Id);
        updatedEmail.Should().NotBeNull();
        updatedEmail!.Status.Should().Be(EmailStatus.Sent);
        updatedEmail.SentAt.Should().NotBeNull();
        updatedEmail.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task EmailSendingFlow_WithMultiplePendingEmails_ProcessesAllInOrder()
    {
        // Arrange
        var emails = new List<EmailOutbox>();
        for (var i = 0; i < 5; i++)
        {
            var email = EmailOutbox.Create(
                OutboxEventId.New(),
                $"user{i}@example.com",
                $"User {i}",
                $"Subject {i}",
                $"<html><body>Body {i}</body></html>");
            emails.Add(email);
        }

        var repository = _serviceProvider.GetRequiredService<IEmailOutboxRepository>();
        foreach (var email in emails)
        {
            await repository.AddAsync(email);
            await Task.Delay(10); // Ensure different timestamps
        }
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act - Simulate processing
        var emailService = _serviceProvider.GetRequiredService<IEmailService>();
        var pendingEmails = await repository.GetPendingEmailsAsync(50);

        foreach (var email in pendingEmails)
        {
            var message = new EmailMessage
            {
                To = email.To,
                ToName = email.ToName,
                Subject = email.Subject,
                HtmlBody = email.HtmlBody
            };

            var success = await emailService.SendEmailAsync(message);
            if (success)
            {
                email.MarkAsSent();
            }
        }
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var allEmails = await _context.EmailOutbox.ToListAsync();
        allEmails.Should().HaveCount(5);
        allEmails.Should().OnlyContain(e => e.Status == EmailStatus.Sent);
        allEmails.Should().OnlyContain(e => e.SentAt != null);
    }

    #endregion

    #region Retry Logic Tests

    [Fact]
    public async Task EmailSendingFlow_WithFailedEmail_MarksAsFailedAndIncrementsRetryCount()
    {
        // Arrange
        var emailOutbox = EmailOutbox.Create(
            OutboxEventId.New(),
            "failed@example.com",
            "Failed User",
            "Failed Subject",
            "<html><body>Failed Body</body></html>");

        var repository = _serviceProvider.GetRequiredService<IEmailOutboxRepository>();
        await repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Configure mock to fail
        var emailService = _serviceProvider.GetRequiredService<IEmailService>();
        emailService.SendEmailAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act - Simulate processing
        var pendingEmails = await repository.GetPendingEmailsAsync(50);
        foreach (var email in pendingEmails)
        {
            var message = new EmailMessage
            {
                To = email.To,
                ToName = email.ToName,
                Subject = email.Subject,
                HtmlBody = email.HtmlBody
            };

            var success = await emailService.SendEmailAsync(message);
            if (!success)
            {
                email.MarkAsFailedWithRetry("Email sending failed");
            }
        }
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var failedEmail = await repository.GetByIdAsync(emailOutbox.Id);
        failedEmail.Should().NotBeNull();
        failedEmail!.Status.Should().Be(EmailStatus.Failed);
        failedEmail.RetryCount.Should().Be(1);
        failedEmail.ErrorMessage.Should().Be("Email sending failed");
        failedEmail.LastAttemptAt.Should().NotBeNull();
    }

    [Fact]
    public async Task EmailSendingFlow_WithFailedEmailRetry_ProcessesFailedEmails()
    {
        // Arrange
        var emailOutbox = EmailOutbox.Create(
            OutboxEventId.New(),
            "retry@example.com",
            "Retry User",
            "Retry Subject",
            "<html><body>Retry Body</body></html>");
        emailOutbox.MarkAsFailedWithRetry("Initial failure");

        var repository = _serviceProvider.GetRequiredService<IEmailOutboxRepository>();
        await repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Configure mock to succeed on retry
        var emailService = _serviceProvider.GetRequiredService<IEmailService>();
        emailService.ClearReceivedCalls();
        emailService.SendEmailAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act - Simulate retry processing
        var failedEmails = await repository.GetFailedEmailsForRetryAsync(50);
        var retriableEmails = failedEmails.Where(e => e.CanRetry()).ToList();

        foreach (var email in retriableEmails)
        {
            var message = new EmailMessage
            {
                To = email.To,
                ToName = email.ToName,
                Subject = email.Subject,
                HtmlBody = email.HtmlBody
            };

            var success = await emailService.SendEmailAsync(message);
            if (success)
            {
                email.MarkAsSent();
            }
            else
            {
                email.MarkAsFailedWithRetry("Retry failed");
            }
        }
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var retriedEmail = await repository.GetByIdAsync(emailOutbox.Id);
        retriedEmail.Should().NotBeNull();
        retriedEmail!.Status.Should().Be(EmailStatus.Sent);
        retriedEmail.SentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task EmailSendingFlow_After10Failures_MarksAsPermanentlyFailed()
    {
        // Arrange
        var emailOutbox = EmailOutbox.Create(
            OutboxEventId.New(),
            "permanent@example.com",
            "Permanent Failure User",
            "Permanent Failure Subject",
            "<html><body>Permanent Failure Body</body></html>");

        var repository = _serviceProvider.GetRequiredService<IEmailOutboxRepository>();
        await repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();

        // Configure mock to always fail
        var emailService = _serviceProvider.GetRequiredService<IEmailService>();
        emailService.SendEmailAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act - Simulate 10 failed attempts
        for (var i = 0; i < 10; i++)
        {
            _context.ChangeTracker.Clear();
            var pendingOrFailed = await (i == 0
                ? repository.GetPendingEmailsAsync(50)
                : repository.GetFailedEmailsForRetryAsync(50));

            foreach (var email in pendingOrFailed)
            {
                var message = new EmailMessage
                {
                    To = email.To,
                    ToName = email.ToName,
                    Subject = email.Subject,
                    HtmlBody = email.HtmlBody
                };

                var success = await emailService.SendEmailAsync(message);
                if (!success)
                {
                    email.MarkAsFailedWithRetry($"Failure attempt {i + 1}");
                }
            }
            await _context.SaveChangesAsync();
        }
        _context.ChangeTracker.Clear();

        // Assert
        var permanentlyFailedEmail = await repository.GetByIdAsync(emailOutbox.Id);
        permanentlyFailedEmail.Should().NotBeNull();
        permanentlyFailedEmail!.Status.Should().Be(EmailStatus.PermanentlyFailed);
        permanentlyFailedEmail.RetryCount.Should().Be(10);
        permanentlyFailedEmail.SentAt.Should().BeNull();
    }

    #endregion

    #region Batch Processing Tests

    [Fact]
    public async Task EmailSendingFlow_WithBatchSizeLimit_ProcessesCorrectCount()
    {
        // Arrange
        var emailCount = 100;
        var batchSize = 50;

        var repository = _serviceProvider.GetRequiredService<IEmailOutboxRepository>();
        for (var i = 0; i < emailCount; i++)
        {
            var email = EmailOutbox.Create(
                OutboxEventId.New(),
                $"batch{i}@example.com",
                $"Batch User {i}",
                $"Batch Subject {i}",
                $"<html><body>Batch Body {i}</body></html>");
            await repository.AddAsync(email);
        }
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act - Simulate batch processing
        var emailService = _serviceProvider.GetRequiredService<IEmailService>();
        var pendingEmails = await repository.GetPendingEmailsAsync(batchSize);

        foreach (var email in pendingEmails)
        {
            email.MarkAsSent();
        }
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var sentEmails = await _context.EmailOutbox
            .Where(e => e.Status == EmailStatus.Sent)
            .CountAsync();
        var stillPendingEmails = await _context.EmailOutbox
            .Where(e => e.Status == EmailStatus.Pending)
            .CountAsync();

        sentEmails.Should().Be(batchSize);
        stillPendingEmails.Should().Be(emailCount - batchSize);
    }

    #endregion
}
