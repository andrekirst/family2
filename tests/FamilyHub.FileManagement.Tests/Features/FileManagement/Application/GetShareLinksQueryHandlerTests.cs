using FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinks;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetShareLinksQueryHandlerTests
{
    private readonly IShareLinkRepository _repo = Substitute.For<IShareLinkRepository>();
    private readonly GetShareLinksQueryHandler _handler;

    public GetShareLinksQueryHandlerTests()
    {
        _handler = new GetShareLinksQueryHandler(_repo);
    }

    [Fact]
    public async Task Handle_ShouldReturnLinks()
    {
        var familyId = FamilyId.New();
        _repo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([
                ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), familyId, UserId.New(), null, null, null),
                ShareLink.Create(ShareResourceType.Folder, Guid.NewGuid(), familyId, UserId.New(), null, null, null)
            ]);

        var query = new GetShareLinksQuery()
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_Empty_ShouldReturnEmpty()
    {
        _repo.GetByFamilyIdAsync(FamilyId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(new List<ShareLink>());

        var query = new GetShareLinksQuery()
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
        var familyId1 = FamilyId.New();
        _repo.GetByFamilyIdAsync(familyId1, Arg.Any<CancellationToken>())
            .Returns([
                ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), familyId1, UserId.New(), null, null, null)
            ]);

        var query = new GetShareLinksQuery()
        {
            FamilyId = familyId1,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
