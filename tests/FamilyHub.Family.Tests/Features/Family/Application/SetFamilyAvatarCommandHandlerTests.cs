using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Application.Commands.SetFamilyAvatar;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.Avatar;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

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

        var (handler, _, _) = CreateHandler(member, avatar);
        var command = new SetFamilyAvatarCommand(userId, familyId, avatar.Id);

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
        var (handler, _, _) = CreateHandler(member: null, avatar);
        var command = new SetFamilyAvatarCommand(UserId.New(), FamilyId.New(), avatar.Id);

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

        var (handler, _, _) = CreateHandler(member, existingAvatar: null);
        var command = new SetFamilyAvatarCommand(userId, familyId, AvatarId.New());

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
        FakeFamilyMemberRepository MemberRepo,
        FakeAvatarRepository AvatarRepo
    ) CreateHandler(FamilyMember? member, AvatarAggregate? existingAvatar)
    {
        var memberRepo = new FakeFamilyMemberRepository(member);
        var avatarRepo = new FakeAvatarRepository(existingAvatar);
        var handler = new SetFamilyAvatarCommandHandler(memberRepo, avatarRepo);
        return (handler, memberRepo, avatarRepo);
    }
}
