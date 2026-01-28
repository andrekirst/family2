using FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Family.Domain.Enums;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.ValueObjects;
using FamilyHub.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Tests.Integration.Family.Persistence;

/// <summary>
/// Integration tests for EmailOutboxRepository using real PostgreSQL.
/// Tests repository operations, EmailOutbox status transitions, and query filtering.
/// </summary>
[Collection("FamilyDatabase")]
public sealed class EmailOutboxRepositoryIntegrationTests(FamilyPostgreSqlContainerFixture fixture) : IAsyncLifetime
{
    private FamilyDbContext _context = null!;
    private IEmailOutboxRepository _repository = null!;
    private ServiceProvider _serviceProvider = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        _context = new FamilyDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        // Setup service collection to get internal repository implementation
        var services = new ServiceCollection();
        services.AddScoped(_ => _context);
        services.AddScoped<IEmailOutboxRepository>(sp =>
        {
            var context = sp.GetRequiredService<FamilyDbContext>();
            return (IEmailOutboxRepository)Activator.CreateInstance(
                Type.GetType("FamilyHub.Modules.Family.Persistence.Repositories.EmailOutboxRepository, FamilyHub.Modules.Family")!,
                context)!;
        });
        _serviceProvider = services.BuildServiceProvider();
        _repository = _serviceProvider.GetRequiredService<IEmailOutboxRepository>();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidEmailOutbox_PersistsSuccessfully()
    {
        // Arrange
        var emailOutbox = EmailOutbox.Create(
            OutboxEventId.New(),
            "test@example.com",
            "Test User",
            "Test Subject",
            "<html><body>Test Body</body></html>",
            "Test Body");

        // Act
        await _repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(emailOutbox.Id);
        retrieved.Should().NotBeNull();
        retrieved!.To.Should().Be("test@example.com");
        retrieved.ToName.Should().Be("Test User");
        retrieved.Subject.Should().Be("Test Subject");
        retrieved.Status.Should().Be(EmailStatus.PENDING);
    }

    [Fact]
    public async Task AddAsync_WithVogenValueObjects_PersistsCorrectly()
    {
        // Arrange
        var outboxEventId = OutboxEventId.New();
        var emailOutbox = EmailOutbox.Create(
            outboxEventId,
            "vogen@example.com",
            "Vogen Test",
            "Vogen Subject",
            "<html>Vogen Body</html>");

        // Act
        await _repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var retrieved = await _repository.GetByIdAsync(emailOutbox.Id);
        retrieved.Should().NotBeNull();
        retrieved!.OutboxEventId.Should().Be(outboxEventId);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingEmail_ReturnsEmail()
    {
        // Arrange
        var emailOutbox = EmailOutbox.Create(
            OutboxEventId.New(),
            "existing@example.com",
            "Existing User",
            "Existing Subject",
            "<html>Body</html>");

        await _repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(emailOutbox.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(emailOutbox.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentEmail_ReturnsNull()
    {
        // Arrange
        var nonExistentId = EmailOutboxId.New();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPendingEmailsAsync Tests

    [Fact]
    public async Task GetPendingEmailsAsync_WithPendingEmails_ReturnsInCreatedOrder()
    {
        // Arrange
        var email1 = EmailOutbox.Create(
            OutboxEventId.New(),
            "pending1@example.com",
            "User 1",
            "Subject 1",
            "<html>Body 1</html>");

        var email2 = EmailOutbox.Create(
            OutboxEventId.New(),
            "pending2@example.com",
            "User 2",
            "Subject 2",
            "<html>Body 2</html>");

        await _repository.AddAsync(email1);
        await _context.SaveChangesAsync();

        // Small delay to ensure different timestamps
        await Task.Delay(10);

        await _repository.AddAsync(email2);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetPendingEmailsAsync(10);

        // Assert
        results.Should().HaveCount(2);
        results[0].To.Should().Be("pending1@example.com");
        results[1].To.Should().Be("pending2@example.com");
    }

    [Fact]
    public async Task GetPendingEmailsAsync_RespectsBatchSize()
    {
        // Arrange
        for (var i = 0; i < 5; i++)
        {
            var email = EmailOutbox.Create(
                OutboxEventId.New(),
                $"batch{i}@example.com",
                $"User {i}",
                $"Subject {i}",
                $"<html>Body {i}</html>");
            await _repository.AddAsync(email);
        }
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetPendingEmailsAsync(3);

        // Assert
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetPendingEmailsAsync_ExcludesSentEmails()
    {
        // Arrange
        var pendingEmail = EmailOutbox.Create(
            OutboxEventId.New(),
            "pending@example.com",
            "Pending User",
            "Pending Subject",
            "<html>Pending Body</html>");

        var sentEmail = EmailOutbox.Create(
            OutboxEventId.New(),
            "sent@example.com",
            "Sent User",
            "Sent Subject",
            "<html>Sent Body</html>");
        sentEmail.MarkAsSent();

        await _repository.AddAsync(pendingEmail);
        await _repository.AddAsync(sentEmail);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetPendingEmailsAsync(10);

        // Assert
        results.Should().HaveCount(1);
        results[0].To.Should().Be("pending@example.com");
    }

    [Fact]
    public async Task GetPendingEmailsAsync_ExcludesFailedEmails()
    {
        // Arrange
        var pendingEmail = EmailOutbox.Create(
            OutboxEventId.New(),
            "pending@example.com",
            "Pending User",
            "Pending Subject",
            "<html>Pending Body</html>");

        var failedEmail = EmailOutbox.Create(
            OutboxEventId.New(),
            "failed@example.com",
            "Failed User",
            "Failed Subject",
            "<html>Failed Body</html>");
        failedEmail.MarkAsFailedWithRetry("SMTP connection failed");

        await _repository.AddAsync(pendingEmail);
        await _repository.AddAsync(failedEmail);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetPendingEmailsAsync(10);

        // Assert
        results.Should().HaveCount(1);
        results[0].To.Should().Be("pending@example.com");
    }

    #endregion

    #region GetFailedEmailsForRetryAsync Tests

    [Fact]
    public async Task GetFailedEmailsForRetryAsync_WithFailedEmails_ReturnsInCreatedOrder()
    {
        // Arrange
        var email1 = EmailOutbox.Create(
            OutboxEventId.New(),
            "failed1@example.com",
            "User 1",
            "Subject 1",
            "<html>Body 1</html>");
        email1.MarkAsFailedWithRetry("Error 1");

        var email2 = EmailOutbox.Create(
            OutboxEventId.New(),
            "failed2@example.com",
            "User 2",
            "Subject 2",
            "<html>Body 2</html>");
        email2.MarkAsFailedWithRetry("Error 2");

        await _repository.AddAsync(email1);
        await _context.SaveChangesAsync();

        await Task.Delay(10);

        await _repository.AddAsync(email2);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetFailedEmailsForRetryAsync(10);

        // Assert
        results.Should().HaveCount(2);
        results[0].To.Should().Be("failed1@example.com");
        results[1].To.Should().Be("failed2@example.com");
    }

    [Fact]
    public async Task GetFailedEmailsForRetryAsync_ExcludesPendingEmails()
    {
        // Arrange
        var pendingEmail = EmailOutbox.Create(
            OutboxEventId.New(),
            "pending@example.com",
            "Pending User",
            "Pending Subject",
            "<html>Pending Body</html>");

        var failedEmail = EmailOutbox.Create(
            OutboxEventId.New(),
            "failed@example.com",
            "Failed User",
            "Failed Subject",
            "<html>Failed Body</html>");
        failedEmail.MarkAsFailedWithRetry("Error");

        await _repository.AddAsync(pendingEmail);
        await _repository.AddAsync(failedEmail);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetFailedEmailsForRetryAsync(10);

        // Assert
        results.Should().HaveCount(1);
        results[0].To.Should().Be("failed@example.com");
    }

    [Fact]
    public async Task GetFailedEmailsForRetryAsync_ExcludesPermanentlyFailedEmails()
    {
        // Arrange
        var failedEmail = EmailOutbox.Create(
            OutboxEventId.New(),
            "failed@example.com",
            "Failed User",
            "Failed Subject",
            "<html>Failed Body</html>");
        failedEmail.MarkAsFailedWithRetry("Error");

        var permanentlyFailedEmail = EmailOutbox.Create(
            OutboxEventId.New(),
            "permanent@example.com",
            "Permanent User",
            "Permanent Subject",
            "<html>Permanent Body</html>");

        // Mark as failed 10 times to reach permanent failure
        for (var i = 0; i < 10; i++)
        {
            permanentlyFailedEmail.MarkAsFailedWithRetry($"Error {i}");
        }

        await _repository.AddAsync(failedEmail);
        await _repository.AddAsync(permanentlyFailedEmail);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetFailedEmailsForRetryAsync(10);

        // Assert
        results.Should().HaveCount(1);
        results[0].To.Should().Be("failed@example.com");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithStatusChange_PersistsCorrectly()
    {
        // Arrange
        var emailOutbox = EmailOutbox.Create(
            OutboxEventId.New(),
            "update@example.com",
            "Update User",
            "Update Subject",
            "<html>Update Body</html>");

        await _repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();

        // Act
        emailOutbox.MarkAsSent();
        await _repository.UpdateAsync(emailOutbox);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var retrieved = await _repository.GetByIdAsync(emailOutbox.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(EmailStatus.SENT);
        retrieved.SentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithFailedStatus_PersistsErrorMessage()
    {
        // Arrange
        var emailOutbox = EmailOutbox.Create(
            OutboxEventId.New(),
            "error@example.com",
            "Error User",
            "Error Subject",
            "<html>Error Body</html>");

        await _repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();

        // Act
        emailOutbox.MarkAsFailedWithRetry("SMTP connection timeout");
        await _repository.UpdateAsync(emailOutbox);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var retrieved = await _repository.GetByIdAsync(emailOutbox.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(EmailStatus.FAILED);
        retrieved.ErrorMessage.Should().Be("SMTP connection timeout");
        retrieved.RetryCount.Should().Be(1);
        retrieved.LastAttemptAt.Should().NotBeNull();
    }

    #endregion

    #region EmailOutbox Status Transition Tests

    [Fact]
    public async Task EmailOutbox_MarkAsSent_UpdatesStatusAndTimestamp()
    {
        // Arrange
        var emailOutbox = EmailOutbox.Create(
            OutboxEventId.New(),
            "sent@example.com",
            "Sent User",
            "Sent Subject",
            "<html>Sent Body</html>");

        await _repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();

        // Act
        emailOutbox.MarkAsSent();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var retrieved = await _repository.GetByIdAsync(emailOutbox.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(EmailStatus.SENT);
        retrieved.SentAt.Should().NotBeNull();
        retrieved.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task EmailOutbox_MarkAsFailedWithRetry_IncrementsRetryCount()
    {
        // Arrange
        var emailOutbox = EmailOutbox.Create(
            OutboxEventId.New(),
            "retry@example.com",
            "Retry User",
            "Retry Subject",
            "<html>Retry Body</html>");

        await _repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();

        // Act
        emailOutbox.MarkAsFailedWithRetry("First failure");
        await _context.SaveChangesAsync();

        emailOutbox.MarkAsFailedWithRetry("Second failure");
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var retrieved = await _repository.GetByIdAsync(emailOutbox.Id);
        retrieved.Should().NotBeNull();
        retrieved!.RetryCount.Should().Be(2);
        retrieved.ErrorMessage.Should().Be("Second failure");
    }

    [Fact]
    public async Task EmailOutbox_After10Failures_MarkedAsPermanentlyFailed()
    {
        // Arrange
        var emailOutbox = EmailOutbox.Create(
            OutboxEventId.New(),
            "permanent@example.com",
            "Permanent User",
            "Permanent Subject",
            "<html>Permanent Body</html>");

        await _repository.AddAsync(emailOutbox);
        await _context.SaveChangesAsync();

        // Act
        for (var i = 0; i < 10; i++)
        {
            emailOutbox.MarkAsFailedWithRetry($"Failure {i + 1}");
        }
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var retrieved = await _repository.GetByIdAsync(emailOutbox.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(EmailStatus.PERMANENTLY_FAILED);
        retrieved.RetryCount.Should().Be(10);
    }

    #endregion
}
