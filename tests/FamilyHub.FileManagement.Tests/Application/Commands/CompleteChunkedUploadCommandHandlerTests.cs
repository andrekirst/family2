using FamilyHub.Api.Common.Infrastructure.BlobStaging;
using FamilyHub.Api.Features.FileManagement.Application.Commands.CompleteChunkedUpload;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Application.Commands;

public class CompleteChunkedUploadCommandHandlerTests
{
    private readonly IFileManagementStorageService _storageService = Substitute.For<IFileManagementStorageService>();
    private readonly IBlobStagingRepository _blobStagingRepository = Substitute.For<IBlobStagingRepository>();
    private readonly CompleteChunkedUploadCommandHandler _handler;

    private readonly FamilyId _familyId = FamilyId.From(Guid.NewGuid());
    private readonly UserId _userId = UserId.From(Guid.NewGuid());

    private const string TestStorageKey = "completed-upload-key";
    private const string TestUploadId = "upload-abc-123";
    private static readonly string ValidChecksum = new('b', 64);

    public CompleteChunkedUploadCommandHandlerTests()
    {
        _handler = new CompleteChunkedUploadCommandHandler(_storageService, _blobStagingRepository);
    }

    [Fact]
    public async Task Handle_ShouldCompleteUploadAndReturnResult()
    {
        // Arrange
        var expectedResult = new FileStorageResult(
            TestStorageKey,
            MimeType.From("video/mp4"),
            FileSize.From(10485760),
            Checksum.From(ValidChecksum));

        _storageService
            .CompleteChunkedUploadAsync(_familyId, TestUploadId, "video.mp4", CancellationToken.None)
            .ReturnsForAnyArgs(expectedResult);

        var command = new CompleteChunkedUploadCommand(TestUploadId, "video.mp4")
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StorageKey.Should().Be(StorageKey.From(TestStorageKey));
        result.Value.MimeType.Should().Be(MimeType.From("video/mp4"));
        result.Value.Size.Should().Be(FileSize.From(10485760));
        result.Value.Checksum.Should().Be(Checksum.From(ValidChecksum));
    }

    [Fact]
    public async Task Handle_ShouldCreateBlobStagingEntry()
    {
        // Arrange
        var expectedResult = new FileStorageResult(
            TestStorageKey,
            MimeType.From("video/mp4"),
            FileSize.From(10485760),
            Checksum.From(ValidChecksum));

        _storageService
            .CompleteChunkedUploadAsync(_familyId, TestUploadId, "video.mp4", CancellationToken.None)
            .ReturnsForAnyArgs(expectedResult);

        var command = new CompleteChunkedUploadCommand(TestUploadId, "video.mp4")
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _blobStagingRepository.Received(1).AddAsync(
            Arg.Is<BlobStagingEntry>(e =>
                e.Module == "FileManagement" &&
                e.StorageKey == TestStorageKey &&
                e.MaxRetries == 5),
            Arg.Any<CancellationToken>());
    }
}
