using FluentAssertions;
using FamilyHub.Api.Features.Photos.Application.Queries;
using FamilyHub.Api.Features.Photos.Domain.Repositories;
using FamilyHub.Api.Features.Photos.Models;
using FamilyHub.Common.Domain.ValueObjects;
using NSubstitute;

namespace FamilyHub.Photos.Tests.Application;

public class GetPhotosQueryHandlerTests
{
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());

    private static PhotoDto CreateTestPhoto(int index) => new()
    {
        Id = Guid.NewGuid(),
        FamilyId = TestFamilyId.Value,
        UploadedBy = Guid.NewGuid(),
        FileName = $"photo{index}.jpg",
        ContentType = "image/jpeg",
        FileSizeBytes = 1024,
        StoragePath = $"/api/files/{Guid.NewGuid()}/download",
        CreatedAt = DateTime.UtcNow.AddMinutes(-index),
        UpdatedAt = DateTime.UtcNow.AddMinutes(-index)
    };

    [Fact]
    public async Task Handle_ShouldReturnPaginatedPhotos()
    {
        // Arrange
        var photos = Enumerable.Range(0, 5).Select(CreateTestPhoto).ToList();
        var repository = Substitute.For<IPhotoRepository>();
        repository.GetByFamilyAsync(TestFamilyId, 0, 3, Arg.Any<CancellationToken>())
            .Returns(photos.OrderByDescending(p => p.CreatedAt).Take(3).ToList());
        repository.GetCountByFamilyAsync(TestFamilyId, Arg.Any<CancellationToken>())
            .Returns(5);

        var handler = new GetPhotosQueryHandler(repository);
        var query = new GetPhotosQuery(0, 3) { FamilyId = TestFamilyId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(5);
        result.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNoPhotos_ShouldReturnEmptyPage()
    {
        // Arrange
        var repository = Substitute.For<IPhotoRepository>();
        repository.GetByFamilyAsync(TestFamilyId, 0, 30, Arg.Any<CancellationToken>())
            .Returns(new List<PhotoDto>());
        repository.GetCountByFamilyAsync(TestFamilyId, Arg.Any<CancellationToken>())
            .Returns(0);

        var handler = new GetPhotosQueryHandler(repository);
        var query = new GetPhotosQuery(0, 30) { FamilyId = TestFamilyId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.HasMore.Should().BeFalse();
    }
}
