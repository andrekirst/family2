using FamilyHub.Api.Features.FileManagement.Application.Commands.RemovePermission;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class RemovePermissionCommandHandlerTests
{
    private static (RemovePermissionCommandHandler handler, FakeFilePermissionRepository permRepo) CreateHandler()
    {
        var permRepo = new FakeFilePermissionRepository();
        var handler = new RemovePermissionCommandHandler(permRepo);
        return (handler, permRepo);
    }

    [Fact]
    public async Task Handle_ShouldRemovePermission()
    {
        var familyId = FamilyId.New();
        var memberId = UserId.New();
        var resourceId = Guid.NewGuid();
        var (handler, permRepo) = CreateHandler();

        var permission = FilePermission.Create(
            PermissionResourceType.File, resourceId, memberId,
            FilePermissionLevel.View, familyId, UserId.New());
        permRepo.Permissions.Add(permission);

        var command = new RemovePermissionCommand(
            PermissionResourceType.File,
            resourceId,
            memberId,
            familyId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        permRepo.Permissions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenPermissionNotFound()
    {
        var (handler, _) = CreateHandler();

        var command = new RemovePermissionCommand(
            PermissionResourceType.File,
            Guid.NewGuid(),
            UserId.New(),
            FamilyId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenPermissionBelongsToDifferentFamily()
    {
        var memberId = UserId.New();
        var resourceId = Guid.NewGuid();
        var (handler, permRepo) = CreateHandler();

        var permission = FilePermission.Create(
            PermissionResourceType.File, resourceId, memberId,
            FilePermissionLevel.View, FamilyId.New(), UserId.New());
        permRepo.Permissions.Add(permission);

        var command = new RemovePermissionCommand(
            PermissionResourceType.File,
            resourceId,
            memberId,
            FamilyId.New()); // Different family

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
