using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Family.Application.Commands.RemoveAvatar;
using FamilyHub.Api.Common.Infrastructure.Avatar;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Family.Tests.Features.Family.Application;

public class RemoveAvatarCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemoveAvatarAndReturnSuccess()
    {
        // Arrange
        var user = CreateTestUser();
        var avatar = CreateTestAvatar();
        user.SetAvatar(avatar.Id);
        user.ClearDomainEvents();

        var (handler, _, _, _) = CreateHandler(user, existingAvatar: avatar);
        var command = new RemoveAvatarCommand(user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        user.AvatarId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldDeleteStoredFiles()
    {
        // Arrange
        var user = CreateTestUser();
        var avatar = CreateTestAvatar();
        user.SetAvatar(avatar.Id);
        user.ClearDomainEvents();

        var (handler, _, fileStorage, _) = CreateHandler(user, existingAvatar: avatar);
        var command = new RemoveAvatarCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — all variant storage keys deleted
        fileStorage.DeletedKeys.Should().HaveCount(4);
    }

    [Fact]
    public async Task Handle_ShouldDeleteAvatarFromRepository()
    {
        // Arrange
        var user = CreateTestUser();
        var avatar = CreateTestAvatar();
        user.SetAvatar(avatar.Id);
        user.ClearDomainEvents();

        var (handler, avatarRepo, _, _) = CreateHandler(user, existingAvatar: avatar);
        var command = new RemoveAvatarCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        avatarRepo.DeletedAvatarIds.Should().Contain(avatar.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserHasNoAvatar()
    {
        // Arrange
        var user = CreateTestUser();
        var (handler, _, _, _) = CreateHandler(user);
        var command = new RemoveAvatarCommand(user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — no avatar to remove, still succeeds
        result.Success.Should().BeTrue();
        user.AvatarId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenUserNotFound()
    {
        // Arrange
        var userId = UserId.New();
        var (handler, _, _, _) = CreateHandler(user: null);
        var command = new RemoveAvatarCommand(userId);

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

        return AvatarAggregate.Create("avatar.jpg", "image/jpeg", variants);
    }

    private static (
        RemoveAvatarCommandHandler Handler,
        FakeAvatarRepository AvatarRepo,
        FakeFileStorageService FileStorage,
        FakeUserRepository UserRepo
    ) CreateHandler(User? user, AvatarAggregate? existingAvatar = null)
    {
        var userRepo = new FakeUserRepository(user);
        var avatarRepo = new FakeAvatarRepository(existingAvatar);
        var fileStorage = new FakeFileStorageService();
        var handler = new RemoveAvatarCommandHandler(userRepo, avatarRepo, fileStorage);
        return (handler, avatarRepo, fileStorage, userRepo);
    }
}
