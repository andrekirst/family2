using FamilyHub.Api.Features.FileManagement.Application.Queries.GetPermissions;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetPermissionsQueryHandlerTests
{
    private readonly IFilePermissionRepository _permRepo = Substitute.For<IFilePermissionRepository>();
    private readonly GetPermissionsQueryHandler _handler;

    public GetPermissionsQueryHandlerTests()
    {
        _handler = new GetPermissionsQueryHandler(_permRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnPermissionsForResource()
    {
        var familyId = FamilyId.New();
        var resourceId = Guid.NewGuid();

        _permRepo.GetByResourceAsync(PermissionResourceType.File, resourceId, Arg.Any<CancellationToken>())
            .Returns([
                FilePermission.Create(PermissionResourceType.File, resourceId, UserId.New(), FilePermissionLevel.View, familyId, UserId.New(), DateTimeOffset.UtcNow),
                FilePermission.Create(PermissionResourceType.File, resourceId, UserId.New(), FilePermissionLevel.Edit, familyId, UserId.New(), DateTimeOffset.UtcNow)
            ]);

        var query = new GetPermissionsQuery(PermissionResourceType.File, resourceId)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoPermissions()
    {
        _permRepo.GetByResourceAsync(Arg.Any<PermissionResourceType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<FilePermission>());

        var query = new GetPermissionsQuery(PermissionResourceType.File, Guid.NewGuid())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldFilterByFamily()
    {
        var familyId = FamilyId.New();
        var resourceId = Guid.NewGuid();

        // The repository returns all for the resource; the handler filters by family
        var perm1 = FilePermission.Create(PermissionResourceType.File, resourceId, UserId.New(), FilePermissionLevel.View, familyId, UserId.New(), DateTimeOffset.UtcNow);
        var perm2 = FilePermission.Create(PermissionResourceType.File, resourceId, UserId.New(), FilePermissionLevel.Edit, FamilyId.New(), UserId.New(), DateTimeOffset.UtcNow);

        _permRepo.GetByResourceAsync(PermissionResourceType.File, resourceId, Arg.Any<CancellationToken>())
            .Returns([perm1, perm2]);

        var query = new GetPermissionsQuery(PermissionResourceType.File, resourceId)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
