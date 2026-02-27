using FluentAssertions;
using FamilyHub.Api.Features.Photos.Application.Commands;
using FamilyHub.Api.Features.Photos.Application.Handlers;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;

namespace FamilyHub.Photos.Tests.Application;

public class UploadPhotoCommandHandlerTests
{
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());

    [Fact]
    public async Task Handle_ShouldCreatePhotoAndReturnId()
    {
        // Arrange
        var repository = new FakePhotoRepository();
        var handler = new UploadPhotoCommandHandler(repository);
        var command = new UploadPhotoCommand(
            TestFamilyId,
            TestUserId,
            "vacation.jpg",
            "image/jpeg",
            2048,
            "/uploads/vacation.jpg",
            null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.PhotoId.Value.Should().NotBe(Guid.Empty);
        repository.AddedPhotos.Should().ContainSingle()
            .Which.FileName.Should().Be("vacation.jpg");
    }

    [Fact]
    public async Task Handle_WithCaption_ShouldSetCaption()
    {
        // Arrange
        var repository = new FakePhotoRepository();
        var handler = new UploadPhotoCommandHandler(repository);
        var caption = PhotoCaption.From("Beach sunset");
        var command = new UploadPhotoCommand(
            TestFamilyId,
            TestUserId,
            "sunset.jpg",
            "image/jpeg",
            4096,
            "/uploads/sunset.jpg",
            caption);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.AddedPhotos.Should().ContainSingle()
            .Which.Caption.Should().Be(caption);
    }
}
