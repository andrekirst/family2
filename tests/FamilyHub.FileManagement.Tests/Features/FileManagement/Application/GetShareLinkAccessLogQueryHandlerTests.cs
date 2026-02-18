using FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinkAccessLog;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetShareLinkAccessLogQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnLogs()
    {
        var linkRepo = new FakeShareLinkRepository();
        var logRepo = new FakeShareLinkAccessLogRepository();
        var handler = new GetShareLinkAccessLogQueryHandler(linkRepo, logRepo);

        var familyId = FamilyId.New();
        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), familyId, UserId.New(), null, null, null);
        linkRepo.Links.Add(link);

        logRepo.Logs.Add(ShareLinkAccessLog.Create(link.Id, "10.0.0.1", "Mozilla/5.0", ShareAccessAction.View));
        logRepo.Logs.Add(ShareLinkAccessLog.Create(link.Id, "10.0.0.2", null, ShareAccessAction.Download));

        var query = new GetShareLinkAccessLogQuery(link.Id, familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_LinkNotFound_ShouldThrow()
    {
        var linkRepo = new FakeShareLinkRepository();
        var logRepo = new FakeShareLinkAccessLogRepository();
        var handler = new GetShareLinkAccessLogQueryHandler(linkRepo, logRepo);

        var query = new GetShareLinkAccessLogQuery(ShareLinkId.New(), FamilyId.New());
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Share link not found*");
    }

    [Fact]
    public async Task Handle_DifferentFamily_ShouldThrow()
    {
        var linkRepo = new FakeShareLinkRepository();
        var logRepo = new FakeShareLinkAccessLogRepository();
        var handler = new GetShareLinkAccessLogQueryHandler(linkRepo, logRepo);

        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, null, null);
        linkRepo.Links.Add(link);

        var query = new GetShareLinkAccessLogQuery(link.Id, FamilyId.New()); // Different family
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Share link not found*");
    }
}
