using FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinkAccessLog;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetShareLinkAccessLogQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnLogs()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new GetShareLinkAccessLogQueryHandler(linkRepo, logRepo);

        var familyId = FamilyId.New();
        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), familyId, UserId.New(), null, null, null);
        linkRepo.GetByIdAsync(link.Id, Arg.Any<CancellationToken>()).Returns(link);

        var logs = new List<ShareLinkAccessLog>
        {
            ShareLinkAccessLog.Create(link.Id, "10.0.0.1", "Mozilla/5.0", ShareAccessAction.View),
            ShareLinkAccessLog.Create(link.Id, "10.0.0.2", null, ShareAccessAction.Download)
        };
        logRepo.GetByShareLinkIdAsync(link.Id, Arg.Any<CancellationToken>()).Returns(logs);

        var query = new GetShareLinkAccessLogQuery(link.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_LinkNotFound_ShouldThrow()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new GetShareLinkAccessLogQueryHandler(linkRepo, logRepo);

        linkRepo.GetByIdAsync(ShareLinkId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((ShareLink?)null);

        var query = new GetShareLinkAccessLogQuery(ShareLinkId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Share link not found*");
    }

    [Fact]
    public async Task Handle_DifferentFamily_ShouldThrow()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new GetShareLinkAccessLogQueryHandler(linkRepo, logRepo);

        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, null, null);
        linkRepo.GetByIdAsync(link.Id, Arg.Any<CancellationToken>()).Returns(link);

        var query = new GetShareLinkAccessLogQuery(link.Id)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        }; // Different family
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Share link not found*");
    }
}
