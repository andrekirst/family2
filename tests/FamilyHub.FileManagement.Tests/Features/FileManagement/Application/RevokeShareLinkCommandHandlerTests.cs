using FamilyHub.Api.Features.FileManagement.Application.Commands.RevokeShareLink;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class RevokeShareLinkCommandHandlerTests
{
    private readonly IShareLinkRepository _repo = Substitute.For<IShareLinkRepository>();
    private readonly RevokeShareLinkCommandHandler _handler;

    public RevokeShareLinkCommandHandlerTests()
    {
        _handler = new RevokeShareLinkCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_ShouldRevokeShareLink()
    {
        var familyId = FamilyId.New();
        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), familyId, UserId.New(), null, null, null, DateTimeOffset.UtcNow);
        _repo.GetByIdAsync(link.Id, Arg.Any<CancellationToken>()).Returns(link);

        var command = new RevokeShareLinkCommand(link.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        link.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrow()
    {
        _repo.GetByIdAsync(ShareLinkId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((ShareLink?)null);

        var command = new RevokeShareLinkCommand(ShareLinkId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Share link not found*");
    }

    [Fact]
    public async Task Handle_DifferentFamily_ShouldThrow()
    {
        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, null, null, DateTimeOffset.UtcNow);
        _repo.GetByIdAsync(link.Id, Arg.Any<CancellationToken>()).Returns(link);

        var command = new RevokeShareLinkCommand(link.Id)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Share link not found*");
    }
}
