using FamilyHub.Api.Features.FileManagement.Application.Queries.GetZipJobs;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using FamilyHub.TestCommon.Fakes;

namespace FamilyHub.FileManagement.Tests.Application.Queries;

public class GetZipJobsQueryHandlerTests
{
    private readonly FakeZipJobRepository _zipJobRepository = new();
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
        _zipJobRepository.Jobs.Add(
            ZipJob.Create(_familyId, _userId, [Guid.NewGuid(), Guid.NewGuid()]));
        _zipJobRepository.Jobs.Add(
            ZipJob.Create(_familyId, _userId, [Guid.NewGuid()]));

        var query = new GetZipJobsQuery(_familyId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(j => j.FileCount).Should().BeEquivalentTo([2, 1]);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyForUnknownFamily()
    {
        var query = new GetZipJobsQuery(FamilyId.From(Guid.NewGuid()));
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldNotReturnJobsFromOtherFamilies()
    {
        var otherFamilyId = FamilyId.From(Guid.NewGuid());
        _zipJobRepository.Jobs.Add(
            ZipJob.Create(otherFamilyId, _userId, [Guid.NewGuid()]));

        var query = new GetZipJobsQuery(_familyId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
