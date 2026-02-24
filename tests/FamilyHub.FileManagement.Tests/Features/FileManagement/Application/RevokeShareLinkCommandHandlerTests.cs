using FamilyHub.Api.Features.FileManagement.Application.Commands.RevokeShareLink;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class RevokeShareLinkCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRevokeShareLink()
    {
        var repo = new FakeShareLinkRepository();
        var handler = new RevokeShareLinkCommandHandler(repo);

        var familyId = FamilyId.New();
        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), familyId, UserId.New(), null, null, null);
        repo.Links.Add(link);

        var command = new RevokeShareLinkCommand(link.Id, familyId, UserId.New());
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        link.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrow()
    {
        var repo = new FakeShareLinkRepository();
        var handler = new RevokeShareLinkCommandHandler(repo);

        var command = new RevokeShareLinkCommand(ShareLinkId.New(), FamilyId.New(), UserId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Share link not found*");
    }

    [Fact]
    public async Task Handle_DifferentFamily_ShouldThrow()
    {
        var repo = new FakeShareLinkRepository();
        var handler = new RevokeShareLinkCommandHandler(repo);

        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, null, null);
        repo.Links.Add(link);

        var command = new RevokeShareLinkCommand(link.Id, FamilyId.New(), UserId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Share link not found*");
    }
}
