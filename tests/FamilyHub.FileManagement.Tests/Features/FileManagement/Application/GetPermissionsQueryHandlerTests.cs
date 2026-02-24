using FamilyHub.Api.Features.FileManagement.Application.Queries.GetPermissions;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetPermissionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPermissionsForResource()
    {
        var permRepo = new FakeFilePermissionRepository();
        var handler = new GetPermissionsQueryHandler(permRepo);

        var familyId = FamilyId.New();
        var resourceId = Guid.NewGuid();

        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.File, resourceId, UserId.New(),
            FilePermissionLevel.View, familyId, UserId.New()));
        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.File, resourceId, UserId.New(),
            FilePermissionLevel.Edit, familyId, UserId.New()));

        var query = new GetPermissionsQuery(PermissionResourceType.File, resourceId, familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoPermissions()
    {
        var permRepo = new FakeFilePermissionRepository();
        var handler = new GetPermissionsQueryHandler(permRepo);

        var query = new GetPermissionsQuery(PermissionResourceType.File, Guid.NewGuid(), FamilyId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldFilterByFamily()
    {
        var permRepo = new FakeFilePermissionRepository();
        var handler = new GetPermissionsQueryHandler(permRepo);

        var familyId = FamilyId.New();
        var otherFamilyId = FamilyId.New();
        var resourceId = Guid.NewGuid();

        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.File, resourceId, UserId.New(),
            FilePermissionLevel.View, familyId, UserId.New()));
        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.File, resourceId, UserId.New(),
            FilePermissionLevel.Edit, otherFamilyId, UserId.New()));

        var query = new GetPermissionsQuery(PermissionResourceType.File, resourceId, familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
