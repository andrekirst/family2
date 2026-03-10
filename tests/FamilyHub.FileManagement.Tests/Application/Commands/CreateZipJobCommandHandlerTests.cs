using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateZipJob;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Application.Commands;

public class CreateZipJobCommandHandlerTests
{
    private readonly IZipJobRepository _zipJobRepository = Substitute.For<IZipJobRepository>();
    private readonly CreateZipJobCommandHandler _handler;

    private readonly FamilyId _familyId = FamilyId.From(Guid.NewGuid());
    private readonly UserId _userId = UserId.From(Guid.NewGuid());

    public CreateZipJobCommandHandlerTests()
    {
        _handler = new CreateZipJobCommandHandler(_zipJobRepository, TimeProvider.System);
    }

    [Fact]
    public async Task Handle_ShouldCreateZipJob()
    {
        var fileIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _zipJobRepository.GetActiveJobCountAsync(_familyId, Arg.Any<CancellationToken>()).Returns(0);

        var command = new CreateZipJobCommand(fileIds)
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.JobId.Should().NotBeEmpty();
        result.Value.Status.Should().Be(ZipJobStatus.Pending.ToString());
        await _zipJobRepository.Received(1).AddAsync(
            Arg.Is<Api.Features.FileManagement.Domain.Entities.ZipJob>(j => j.FileIds.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRejectWhenTooManyFiles()
    {
        var fileIds = Enumerable.Range(0, 1001).Select(_ => Guid.NewGuid()).ToList();
        var command = new CreateZipJobCommand(fileIds)
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Match("*1000*");
    }

    [Fact]
    public async Task Handle_ShouldRejectWhenConcurrentLimitReached()
    {
        _zipJobRepository.GetActiveJobCountAsync(_familyId, Arg.Any<CancellationToken>()).Returns(3);

        var command = new CreateZipJobCommand([Guid.NewGuid()])
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Match("*3*concurrent*");
    }
}
