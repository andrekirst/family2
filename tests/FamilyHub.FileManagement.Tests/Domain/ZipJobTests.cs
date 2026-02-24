using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Domain;

public class ZipJobTests
{
    private readonly FamilyId _familyId = FamilyId.From(Guid.NewGuid());
    private readonly UserId _userId = UserId.From(Guid.NewGuid());
    private readonly List<Guid> _fileIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

    [Fact]
    public void Create_ShouldInitializeWithPendingStatus()
    {
        var job = ZipJob.Create(_familyId, _userId, _fileIds);

        job.FamilyId.Should().Be(_familyId);
        job.InitiatedBy.Should().Be(_userId);
        job.FileIds.Should().HaveCount(3);
        job.Status.Should().Be(ZipJobStatus.Pending);
        job.Progress.Should().Be(0);
        job.ZipStorageKey.Should().BeNull();
        job.ZipSize.Should().BeNull();
        job.ErrorMessage.Should().BeNull();
        job.CompletedAt.Should().BeNull();
        job.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddHours(23));
    }

    [Fact]
    public void Create_WithCustomTtl_ShouldSetCorrectExpiry()
    {
        var job = ZipJob.Create(_familyId, _userId, _fileIds, TimeSpan.FromHours(1));

        job.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkProcessing_ShouldTransitionToProcessingStatus()
    {
        var job = ZipJob.Create(_familyId, _userId, _fileIds);

        job.MarkProcessing();

        job.Status.Should().Be(ZipJobStatus.Processing);
    }

    [Fact]
    public void UpdateProgress_ShouldClampBetween0And100()
    {
        var job = ZipJob.Create(_familyId, _userId, _fileIds);

        job.UpdateProgress(50);
        job.Progress.Should().Be(50);

        job.UpdateProgress(-10);
        job.Progress.Should().Be(0);

        job.UpdateProgress(150);
        job.Progress.Should().Be(100);
    }

    [Fact]
    public void MarkCompleted_ShouldSetStorageKeyAndRaiseEvent()
    {
        var job = ZipJob.Create(_familyId, _userId, _fileIds);
        var storageKey = StorageKey.From("zip/test.zip");

        job.MarkCompleted(storageKey, 1024);

        job.Status.Should().Be(ZipJobStatus.Completed);
        job.Progress.Should().Be(100);
        job.ZipStorageKey.Should().Be(storageKey);
        job.ZipSize.Should().Be(1024);
        job.CompletedAt.Should().NotBeNull();

        var domainEvent = job.DomainEvents
            .OfType<ZipJobCompletedEvent>()
            .Should().ContainSingle().Subject;
        domainEvent.ZipJobId.Should().Be(job.Id);
        domainEvent.FileCount.Should().Be(3);
        domainEvent.ZipSize.Should().Be(1024);
        domainEvent.FamilyId.Should().Be(_familyId);
    }

    [Fact]
    public void MarkFailed_ShouldSetErrorMessage()
    {
        var job = ZipJob.Create(_familyId, _userId, _fileIds);

        job.MarkFailed("Storage error");

        job.Status.Should().Be(ZipJobStatus.Failed);
        job.ErrorMessage.Should().Be("Storage error");
        job.CompletedAt.Should().NotBeNull();
    }
}
