using FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinks;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetShareLinksQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnLinks()
    {
        var repo = new FakeShareLinkRepository();
        var handler = new GetShareLinksQueryHandler(repo);

        var familyId = FamilyId.New();
        repo.Links.Add(ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), familyId, UserId.New(), null, null, null));
        repo.Links.Add(ShareLink.Create(ShareResourceType.Folder, Guid.NewGuid(), familyId, UserId.New(), null, null, null));

        var query = new GetShareLinksQuery(familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_Empty_ShouldReturnEmpty()
    {
        var repo = new FakeShareLinkRepository();
        var handler = new GetShareLinksQueryHandler(repo);

        var query = new GetShareLinksQuery(FamilyId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldFilterByFamily()
    {
        var repo = new FakeShareLinkRepository();
        var handler = new GetShareLinksQueryHandler(repo);

        var familyId1 = FamilyId.New();
        var familyId2 = FamilyId.New();
        repo.Links.Add(ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), familyId1, UserId.New(), null, null, null));
        repo.Links.Add(ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), familyId2, UserId.New(), null, null, null));

        var query = new GetShareLinksQuery(familyId1);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
