using FamilyHub.Api.Features.FileManagement.Application.Commands.RemovePermission;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class RemovePermissionCommandHandlerTests
{
    private readonly IFilePermissionRepository _permRepo = Substitute.For<IFilePermissionRepository>();
    private readonly RemovePermissionCommandHandler _handler;

    public RemovePermissionCommandHandlerTests()
    {
        _handler = new RemovePermissionCommandHandler(_permRepo);
    }

    [Fact]
    public async Task Handle_ShouldRemovePermission()
    {
        var familyId = FamilyId.New();
        var memberId = UserId.New();
        var resourceId = Guid.NewGuid();

        var permission = FilePermission.Create(
            PermissionResourceType.File, resourceId, memberId,
            FilePermissionLevel.View, familyId, UserId.New(), DateTimeOffset.UtcNow);
        _permRepo.GetByMemberAndResourceAsync(memberId, PermissionResourceType.File, resourceId, Arg.Any<CancellationToken>())
            .Returns(permission);

        var command = new RemovePermissionCommand(
            PermissionResourceType.File,
            resourceId,
            memberId)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _permRepo.Received(1).RemoveAsync(permission, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenPermissionNotFound()
    {
        _permRepo.GetByMemberAndResourceAsync(UserId.New(), Arg.Any<PermissionResourceType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((FilePermission?)null);

        var command = new RemovePermissionCommand(
            PermissionResourceType.File,
            Guid.NewGuid(),
            UserId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenPermissionBelongsToDifferentFamily()
    {
        var memberId = UserId.New();
        var resourceId = Guid.NewGuid();

        var permission = FilePermission.Create(
            PermissionResourceType.File, resourceId, memberId,
            FilePermissionLevel.View, FamilyId.New(), UserId.New(), DateTimeOffset.UtcNow);
        _permRepo.GetByMemberAndResourceAsync(memberId, PermissionResourceType.File, resourceId, Arg.Any<CancellationToken>())
            .Returns(permission);

        var command = new RemovePermissionCommand(
            PermissionResourceType.File,
            resourceId,
            memberId)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        }; // Different family

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
