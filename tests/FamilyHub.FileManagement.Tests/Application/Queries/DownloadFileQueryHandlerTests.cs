using FamilyHub.Api.Features.FileManagement.Application.Queries.DownloadFile;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Application.Queries;

public class DownloadFileQueryHandlerTests
{
    private readonly IFileManagementStorageService _storageService = Substitute.For<IFileManagementStorageService>();
    private readonly DownloadFileQueryHandler _handler;

    private readonly FamilyId _familyId = FamilyId.From(Guid.NewGuid());
    private readonly UserId _userId = UserId.From(Guid.NewGuid());

    public DownloadFileQueryHandlerTests()
    {
        _handler = new DownloadFileQueryHandler(_storageService);
    }

    [Fact]
    public async Task Handle_ShouldReturnFileData_WhenFileExists()
    {
        // Arrange
        var fileData = new MemoryStream([1, 2, 3, 4, 5]);
        var downloadResult = new FileDownloadResult(fileData, "image/png", 5);

        _storageService
            .GetFileAsync("test-key", Arg.Any<CancellationToken>())
            .Returns(downloadResult);

        var query = new DownloadFileQuery("test-key")
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
        result.Value.Size.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenFileDoesNotExist()
    {
        // Arrange
        _storageService
            .GetFileAsync("missing-key", Arg.Any<CancellationToken>())
            .Returns((FileDownloadResult?)null);

        var query = new DownloadFileQuery("missing-key")
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
