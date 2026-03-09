using FamilyHub.Api.Features.FileManagement.Application.Queries.GetZipJobs;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Application.Queries;

public class GetZipJobsQueryHandlerTests
{
    private readonly IZipJobRepository _zipJobRepository = Substitute.For<IZipJobRepository>();
    private readonly GetZipJobsQueryHandler _handler;

    private readonly FamilyId _familyId = FamilyId.From(Guid.NewGuid());
    private readonly UserId _userId = UserId.From(Guid.NewGuid());

    public GetZipJobsQueryHandlerTests()
    {
        _handler = new GetZipJobsQueryHandler(_zipJobRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnJobsForFamily()
    {
        var jobs = new List<ZipJob>
        {
            ZipJob.Create(_familyId, _userId, [Guid.NewGuid(), Guid.NewGuid()]),
            ZipJob.Create(_familyId, _userId, [Guid.NewGuid()])
        };
        _zipJobRepository.GetByFamilyIdAsync(_familyId, Arg.Any<CancellationToken>()).Returns(jobs);

        var query = new GetZipJobsQuery()
        {
            FamilyId = _familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(j => j.FileCount).Should().BeEquivalentTo([2, 1]);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyForUnknownFamily()
    {
        var unknownFamily = FamilyId.From(Guid.NewGuid());
        _zipJobRepository.GetByFamilyIdAsync(unknownFamily, Arg.Any<CancellationToken>())
            .Returns(new List<ZipJob>());

        var query = new GetZipJobsQuery()
        {
            FamilyId = unknownFamily,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldNotReturnJobsFromOtherFamilies()
    {
        _zipJobRepository.GetByFamilyIdAsync(_familyId, Arg.Any<CancellationToken>())
            .Returns(new List<ZipJob>());

        var query = new GetZipJobsQuery()
        {
            FamilyId = _familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
