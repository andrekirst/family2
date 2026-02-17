using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Family.Application.Commands.UploadAvatar;
using FamilyHub.Api.Common.Infrastructure.Avatar;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Family.Tests.Features.Family.Application;

public class UploadAvatarCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAvatarAndReturnResult()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, _, _, _, _) = CreateHandler(user);
        var command = CreateCommand(user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AvatarId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldStoreAllFourVariants()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, _, _, fileStorage, _) = CreateHandler(user);
        var command = CreateCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — 4 size variants stored
        fileStorage.StoredFiles.Should().HaveCount(4);
    }

    [Fact]
    public async Task Handle_ShouldAddAvatarToRepository()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, avatarRepo, _, _, _) = CreateHandler(user);
        var command = CreateCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        avatarRepo.AddedAvatars.Should().HaveCount(1);
        avatarRepo.AddedAvatars[0].Variants.Should().HaveCount(4);
        avatarRepo.AddedAvatars[0].OriginalFileName.Should().Be("test.jpg");
        avatarRepo.AddedAvatars[0].OriginalMimeType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task Handle_ShouldSetUserAvatar()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, _, _, _, _) = CreateHandler(user);
        var command = CreateCommand(user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        user.AvatarId.Should().NotBeNull();
        user.AvatarId.Should().Be(result.AvatarId);
    }

    [Fact]
    public async Task Handle_ShouldDeletePreviousAvatar_WhenUserAlreadyHasOne()
    {
        // Arrange
        var user = CreateTestUser();
        var previousAvatar = CreateTestAvatar();
        user.SetAvatar(previousAvatar.Id);
        user.ClearDomainEvents();

        var (handler, avatarRepo, _, fileStorage, _) = CreateHandler(user, existingAvatar: previousAvatar);
        var command = CreateCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — previous avatar was deleted
        avatarRepo.DeletedAvatarIds.Should().Contain(previousAvatar.Id);
        // Previous variant storage keys should have been deleted
        fileStorage.DeletedKeys.Should().HaveCount(previousAvatar.Variants.Count);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenUserNotFound()
    {
        // Arrange
        var userId = UserId.New();
        var (handler, _, _, _, _) = CreateHandler(user: null);
        var command = CreateCommand(userId);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("User not found");
    }

    // --- Helpers ---

    private static User CreateTestUser()
    {
        var email = Email.From("test@example.com");
        var name = UserName.From("Test User");
        var externalId = ExternalUserId.From("test-external-id");

        var user = User.Register(email, name, externalId, emailVerified: true);
        user.ClearDomainEvents();

        return user;
    }

    private static AvatarAggregate CreateTestAvatar()
    {
        var variants = new Dictionary<AvatarSize, AvatarVariantData>
        {
            [AvatarSize.Tiny] = new("key-tiny", "image/jpeg", 100, 24, 24),
            [AvatarSize.Small] = new("key-small", "image/jpeg", 200, 48, 48),
            [AvatarSize.Medium] = new("key-medium", "image/jpeg", 500, 128, 128),
            [AvatarSize.Large] = new("key-large", "image/jpeg", 1000, 512, 512),
        };

        return AvatarAggregate.Create("previous.jpg", "image/jpeg", variants);
    }

    private static UploadAvatarCommand CreateCommand(UserId userId) =>
        new(userId, new byte[] { 0xFF, 0xD8, 0xFF }, "test.jpg", "image/jpeg",
            null, null, null, null);

    private static (
        UploadAvatarCommandHandler Handler,
        FakeAvatarRepository AvatarRepo,
        FakeUserRepository UserRepo,
        FakeFileStorageService FileStorage,
        FakeAvatarProcessingService Processing
    ) CreateHandler(User? user, AvatarAggregate? existingAvatar = null)
    {
        var userRepo = new FakeUserRepository(user);
        var avatarRepo = new FakeAvatarRepository(existingAvatar);
        var fileStorage = new FakeFileStorageService();
        var processing = new FakeAvatarProcessingService();
        var handler = new UploadAvatarCommandHandler(userRepo, avatarRepo, processing, fileStorage);
        return (handler, avatarRepo, userRepo, fileStorage, processing);
    }
}
