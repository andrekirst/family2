using FamilyHub.Api.Features.FileManagement.Application.Queries.StreamFile;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Application.Queries;

public class StreamFileQueryHandlerTests
{
    private readonly IFileManagementStorageService _storageService = Substitute.For<IFileManagementStorageService>();
    private readonly StreamFileQueryHandler _handler;

    private readonly FamilyId _familyId = FamilyId.From(Guid.NewGuid());
    private readonly UserId _userId = UserId.From(Guid.NewGuid());

    public StreamFileQueryHandlerTests()
    {
        _handler = new StreamFileQueryHandler(_storageService);
    }

    [Fact]
    public async Task Handle_ShouldReturnFullFile_WhenNoRangeRequested()
    {
        // Arrange
        var fileData = new MemoryStream([1, 2, 3]);
        var downloadResult = new FileDownloadResult(fileData, "image/png", 3);

        _storageService
            .GetFileAsync("full-key", Arg.Any<CancellationToken>())
            .Returns(downloadResult);

        var query = new StreamFileQuery("full-key", RangeFrom: null, RangeTo: null)
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().BeSameAs(fileData);
        result.Value.MimeType.Should().Be("image/png");
        result.Value.ContentLength.Should().Be(3);
        result.Value.IsPartialContent.Should().BeFalse();
        result.Value.RangeStart.Should().BeNull();
        result.Value.RangeEnd.Should().BeNull();
        result.Value.TotalSize.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldReturnPartialContent_WhenRangeRequested()
    {
        // Arrange
        var rangeData = new MemoryStream([10, 20, 30]);
        var rangeResult = new StorageRangeResult(rangeData, RangeStart: 0, RangeEnd: 99, TotalSize: 1000);

        _storageService
            .GetFileRangeAsync("range-key", 0, 99, Arg.Any<CancellationToken>())
            .Returns(rangeResult);

        var query = new StreamFileQuery("range-key", RangeFrom: 0, RangeTo: 99)
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().BeSameAs(rangeData);
        result.Value.IsPartialContent.Should().BeTrue();
        result.Value.RangeStart.Should().Be(0);
        result.Value.RangeEnd.Should().Be(99);
        result.Value.TotalSize.Should().Be(1000);
        result.Value.ContentLength.Should().Be(100); // RangeEnd - RangeStart + 1
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenFileDoesNotExist_FullDownload()
    {
        // Arrange
        _storageService
            .GetFileAsync("missing-key", Arg.Any<CancellationToken>())
            .Returns((FileDownloadResult?)null);

        var query = new StreamFileQuery("missing-key", RangeFrom: null, RangeTo: null)
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenFileDoesNotExist_RangeDownload()
    {
        // Arrange
        _storageService
            .GetFileRangeAsync("missing-key", 0, 99, Arg.Any<CancellationToken>())
            .Returns((StorageRangeResult?)null);

        var query = new StreamFileQuery("missing-key", RangeFrom: 0, RangeTo: 99)
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
    }
}
