using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Application.Commands.SetFamilyAvatar;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.Avatar;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Family.Tests.Features.Family.Application;

public class SetFamilyAvatarCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSetFamilyAvatarAndReturnSuccess()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var member = FamilyMember.Create(familyId, userId, FamilyRole.Member);
        var avatar = CreateTestAvatar();

        var (handler, _, _) = CreateHandler(member, avatar, userId, familyId);
        var command = new SetFamilyAvatarCommand(avatar.Id) { UserId = userId, FamilyId = familyId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        member.AvatarId.Should().Be(avatar.Id);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenMemberNotFound()
    {
        // Arrange
        var avatar = CreateTestAvatar();
        var userId = UserId.New();
        var familyId = FamilyId.New();
        var (handler, _, _) = CreateHandler(member: null, avatar, userId, familyId);
        var command = new SetFamilyAvatarCommand(avatar.Id) { UserId = userId, FamilyId = familyId };

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Family member not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenAvatarNotFound()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var member = FamilyMember.Create(familyId, userId, FamilyRole.Member);
        var avatarId = AvatarId.New();

        var (handler, _, _) = CreateHandler(member, existingAvatar: null, userId, familyId);
        var command = new SetFamilyAvatarCommand(avatarId) { UserId = userId, FamilyId = familyId };

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Avatar not found");
    }

    // --- Helpers ---

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
        SetFamilyAvatarCommandHandler Handler,
        IFamilyMemberRepository MemberRepo,
        IAvatarRepository AvatarRepo
    ) CreateHandler(FamilyMember? member, AvatarAggregate? existingAvatar, UserId userId, FamilyId familyId)
    {
        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        memberRepo.GetByUserAndFamilyAsync(userId, familyId, CancellationToken.None).Returns(member);

        var avatarRepo = Substitute.For<IAvatarRepository>();
        if (existingAvatar is not null)
        {
            avatarRepo.GetByIdAsync(existingAvatar.Id, CancellationToken.None).Returns(existingAvatar);
        }

        var handler = new SetFamilyAvatarCommandHandler(memberRepo, avatarRepo);
        return (handler, memberRepo, avatarRepo);
    }
}
