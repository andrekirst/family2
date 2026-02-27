using FluentAssertions;
using FamilyHub.Api.Features.Photos.Application.Handlers;
using FamilyHub.Api.Features.Photos.Application.Queries;
using FamilyHub.Api.Features.Photos.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;

namespace FamilyHub.Photos.Tests.Application;

public class GetPhotosQueryHandlerTests
{
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());

    [Fact]
    public async Task Handle_ShouldReturnPaginatedPhotos()
    {
        // Arrange
        var repository = new FakePhotoRepository();
        for (var i = 0; i < 5; i++)
        {
            var photo = Photo.Create(
                TestFamilyId, TestUserId, $"photo{i}.jpg", "image/jpeg", 1024, $"/uploads/photo{i}.jpg");
            await repository.AddAsync(photo);
        }
        var handler = new GetPhotosQueryHandler(repository);
        var query = new GetPhotosQuery(TestFamilyId, 0, 3);

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
        var repository = new FakePhotoRepository();
        var handler = new GetPhotosQueryHandler(repository);
        var query = new GetPhotosQuery(TestFamilyId, 0, 30);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldExcludeDeletedPhotos()
    {
        // Arrange
        var repository = new FakePhotoRepository();
        var photo1 = Photo.Create(
            TestFamilyId, TestUserId, "visible.jpg", "image/jpeg", 1024, "/uploads/visible.jpg");
        var photo2 = Photo.Create(
            TestFamilyId, TestUserId, "deleted.jpg", "image/jpeg", 1024, "/uploads/deleted.jpg");
        photo2.SoftDelete(TestUserId);
        await repository.AddAsync(photo1);
        await repository.AddAsync(photo2);

        var handler = new GetPhotosQueryHandler(repository);
        var query = new GetPhotosQuery(TestFamilyId, 0, 30);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle()
            .Which.FileName.Should().Be("visible.jpg");
        result.TotalCount.Should().Be(1);
    }
}
