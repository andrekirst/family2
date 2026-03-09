using FamilyHub.Api.Common.Infrastructure.BlobStaging;
using FamilyHub.Api.Features.FileManagement.Application.Commands.StoreUploadedFile;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Application.Commands;

public class StoreUploadedFileCommandHandlerTests
{
    private readonly IFileManagementStorageService _storageService = Substitute.For<IFileManagementStorageService>();
    private readonly IBlobStagingRepository _blobStagingRepository = Substitute.For<IBlobStagingRepository>();
    private readonly StoreUploadedFileCommandHandler _handler;

    private readonly FamilyId _familyId = FamilyId.From(Guid.NewGuid());
    private readonly UserId _userId = UserId.From(Guid.NewGuid());

    private const string TestStorageKey = "test-storage-key-001";
    private static readonly string ValidChecksum = new('a', 64);

    public StoreUploadedFileCommandHandlerTests()
    {
        _handler = new StoreUploadedFileCommandHandler(_storageService, _blobStagingRepository);
    }

    [Fact]
    public async Task Handle_ShouldStoreFileAndReturnResult()
    {
        // Arrange
        var fileStream = new MemoryStream([1, 2, 3]);
        var expectedResult = new FileStorageResult(
            TestStorageKey,
            MimeType.From("image/png"),
            FileSize.From(1024),
            Checksum.From(ValidChecksum));

        _storageService
            .StoreFileAsync(_familyId, fileStream, "photo.png", CancellationToken.None)
            .ReturnsForAnyArgs(expectedResult);

        var command = new StoreUploadedFileCommand(fileStream, "photo.png")
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StorageKey.Should().Be(StorageKey.From(TestStorageKey));
        result.Value.MimeType.Should().Be(MimeType.From("image/png"));
        result.Value.FileSize.Should().Be(FileSize.From(1024));
        result.Value.Checksum.Should().Be(Checksum.From(ValidChecksum));
    }

    [Fact]
    public async Task Handle_ShouldCreateBlobStagingEntry()
    {
        // Arrange
        var fileStream = new MemoryStream([1, 2, 3]);
        var expectedResult = new FileStorageResult(
            TestStorageKey,
            MimeType.From("image/png"),
            FileSize.From(1024),
            Checksum.From(ValidChecksum));

        _storageService
            .StoreFileAsync(_familyId, fileStream, "photo.png", CancellationToken.None)
            .ReturnsForAnyArgs(expectedResult);

        var command = new StoreUploadedFileCommand(fileStream, "photo.png")
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
