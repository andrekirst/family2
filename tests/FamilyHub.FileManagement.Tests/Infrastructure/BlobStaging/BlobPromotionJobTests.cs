using FamilyHub.Api.Common.Infrastructure.BlobStaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FamilyHub.FileManagement.Tests.Infrastructure.BlobStaging;

public class BlobPromotionJobTests
{
    private readonly IBlobStagingRepository _repository = Substitute.For<IBlobStagingRepository>();
    private readonly IOptions<BlobStagingOptions> _options = Options.Create(new BlobStagingOptions { BatchSize = 10, MaxRetries = 3 });
    private readonly DeadLetterAlertService _deadLetterService;
    private readonly ILogger<BlobPromotionJob> _logger = Substitute.For<ILogger<BlobPromotionJob>>();
    private readonly BlobPromotionJob _job;

    public BlobPromotionJobTests()
    {
        _deadLetterService = new DeadLetterAlertService(Substitute.For<ILogger<DeadLetterAlertService>>());
        _job = new BlobPromotionJob(_repository, _options, _deadLetterService, _logger);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPromotePendingEntries()
    {
        // Arrange
        var entry1 = new BlobStagingEntry { Module = "FileManagement", StorageKey = "key-1", MaxRetries = 5 };
        var entry2 = new BlobStagingEntry { Module = "FileManagement", StorageKey = "key-2", MaxRetries = 5 };
        var entries = new List<BlobStagingEntry> { entry1, entry2 };

        _repository
            .GetPendingAsync(10, Arg.Any<CancellationToken>())
            .Returns(entries);

        // Act
        await _job.ExecuteAsync();

        // Assert
        entry1.Status.Should().Be(BlobStagingStatus.Promoted);
        entry1.PromotedAt.Should().NotBeNull();
        entry2.Status.Should().Be(BlobStagingStatus.Promoted);
        entry2.PromotedAt.Should().NotBeNull();

        await _repository.Received(2).UpdateAsync(Arg.Any<BlobStagingEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMoveToDeadLetter_WhenRetriesExhausted()
    {
        // Arrange
        var entry = new BlobStagingEntry
        {
            Module = "FileManagement",
            StorageKey = "key-fail",
            MaxRetries = 3,
            RetryCount = 2
        };
        var entries = new List<BlobStagingEntry> { entry };

        _repository
            .GetPendingAsync(10, Arg.Any<CancellationToken>())
            .Returns(entries);

        // First UpdateAsync call (the promotion attempt) throws to simulate failure
        _repository
            .UpdateAsync(Arg.Is<BlobStagingEntry>(e => e.Status == BlobStagingStatus.Promoted), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Database error"));

        // Second UpdateAsync call (the error/dead-letter write) succeeds
        _repository
            .UpdateAsync(Arg.Is<BlobStagingEntry>(e => e.Status == BlobStagingStatus.DeadLetter), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _job.ExecuteAsync();

        // Assert
        entry.RetryCount.Should().Be(3);
        entry.Status.Should().Be(BlobStagingStatus.DeadLetter);
        entry.ErrorMessage.Should().Be("Database error");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleEmptyBatch()
    {
        // Arrange
        _repository
            .GetPendingAsync(10, Arg.Any<CancellationToken>())
            .Returns(new List<BlobStagingEntry>());

        // Act
        var act = () => _job.ExecuteAsync();

        // Assert
        await act.Should().NotThrowAsync();
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<BlobStagingEntry>(), Arg.Any<CancellationToken>());
    }
}
