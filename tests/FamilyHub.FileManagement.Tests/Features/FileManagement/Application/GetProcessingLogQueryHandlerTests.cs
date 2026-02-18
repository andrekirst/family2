using FamilyHub.Api.Features.FileManagement.Application.Queries.GetProcessingLog;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetProcessingLogQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnLogEntries()
    {
        var logRepo = new FakeProcessingLogRepository();
        var handler = new GetProcessingLogQueryHandler(logRepo);

        var familyId = FamilyId.New();
        logRepo.Entries.Add(ProcessingLogEntry.Create(
            FileId.New(), "photo.jpg",
            OrganizationRuleId.New(), "Rule",
            RuleActionType.MoveToFolder,
            FolderId.New(), null,
            true, null, familyId));

        logRepo.Entries.Add(ProcessingLogEntry.Create(
            FileId.New(), "doc.pdf",
            null, null, null, null, null,
            true, null, familyId));

        var query = new GetProcessingLogQuery(familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoEntries()
    {
        var logRepo = new FakeProcessingLogRepository();
        var handler = new GetProcessingLogQueryHandler(logRepo);

        var query = new GetProcessingLogQuery(FamilyId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldFilterByFamily()
    {
        var logRepo = new FakeProcessingLogRepository();
        var handler = new GetProcessingLogQueryHandler(logRepo);

        var familyId = FamilyId.New();
        logRepo.Entries.Add(ProcessingLogEntry.Create(
            FileId.New(), "mine.jpg",
            null, null, null, null, null,
            true, null, familyId));

        logRepo.Entries.Add(ProcessingLogEntry.Create(
            FileId.New(), "other.jpg",
            null, null, null, null, null,
            true, null, FamilyId.New()));

        var query = new GetProcessingLogQuery(familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().FileName.Should().Be("mine.jpg");
    }
}
