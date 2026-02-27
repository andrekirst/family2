using FluentAssertions;
using FamilyHub.Api.Features.Photos.Application.Commands;
using FamilyHub.Api.Features.Photos.Application.Handlers;
using FamilyHub.Api.Features.Photos.Domain.Entities;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;

namespace FamilyHub.Photos.Tests.Application;

public class DeletePhotoCommandHandlerTests
{
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());

    [Fact]
    public async Task Handle_ShouldSoftDeletePhoto()
    {
        // Arrange
        var photo = Photo.Create(
            TestFamilyId, TestUserId, "photo.jpg", "image/jpeg", 1024, "/uploads/photo.jpg");
        var repository = new FakePhotoRepository(photo);
        var handler = new DeletePhotoCommandHandler(repository);
        var command = new DeletePhotoCommand(photo.Id, TestUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        photo.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenPhotoNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var repository = new FakePhotoRepository();
        var handler = new DeletePhotoCommandHandler(repository);
        var command = new DeletePhotoCommand(
            Api.Features.Photos.Domain.ValueObjects.PhotoId.From(Guid.NewGuid()),
            TestUserId);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Photo not found");
    }
}
