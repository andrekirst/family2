using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateZipJob;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using FamilyHub.TestCommon.Fakes;

namespace FamilyHub.FileManagement.Tests.Application.Commands;

public class CreateZipJobCommandHandlerTests
{
    private readonly FakeZipJobRepository _zipJobRepository = new();
    private readonly CreateZipJobCommandHandler _handler;

    private readonly FamilyId _familyId = FamilyId.From(Guid.NewGuid());
    private readonly UserId _userId = UserId.From(Guid.NewGuid());

    public CreateZipJobCommandHandlerTests()
    {
        _handler = new CreateZipJobCommandHandler(_zipJobRepository);
    }

    [Fact]
    public async Task Handle_ShouldCreateZipJob()
    {
        var fileIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new CreateZipJobCommand(_familyId, _userId, fileIds);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.JobId.Should().NotBeEmpty();
        result.Status.Should().Be(ZipJobStatus.Pending.ToString());
        _zipJobRepository.Jobs.Should().HaveCount(1);
        _zipJobRepository.Jobs[0].FileIds.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldRejectWhenTooManyFiles()
    {
        var fileIds = Enumerable.Range(0, 1001).Select(_ => Guid.NewGuid()).ToList();
        var command = new CreateZipJobCommand(_familyId, _userId, fileIds);

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*1000*");
    }

    [Fact]
    public async Task Handle_ShouldRejectWhenConcurrentLimitReached()
    {
        // Seed 3 active jobs
        for (var i = 0; i < 3; i++)
        {
            var job = Api.Features.FileManagement.Domain.Entities.ZipJob.Create(
                _familyId, _userId, [Guid.NewGuid()]);
            _zipJobRepository.Jobs.Add(job);
        }

        var command = new CreateZipJobCommand(_familyId, _userId, [Guid.NewGuid()]);

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*3*concurrent*");
    }
}
