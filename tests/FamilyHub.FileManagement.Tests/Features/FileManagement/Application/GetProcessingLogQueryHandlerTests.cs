using FamilyHub.Api.Features.FileManagement.Application.Queries.GetProcessingLog;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetProcessingLogQueryHandlerTests
{
    private readonly IProcessingLogRepository _logRepo = Substitute.For<IProcessingLogRepository>();
    private readonly GetProcessingLogQueryHandler _handler;

    public GetProcessingLogQueryHandlerTests()
    {
        _handler = new GetProcessingLogQueryHandler(_logRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnLogEntries()
    {
        var familyId = FamilyId.New();
        _logRepo.GetByFamilyIdAsync(familyId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([
                ProcessingLogEntry.Create(FileId.New(), "photo.jpg", OrganizationRuleId.New(), "Rule", RuleActionType.MoveToFolder, FolderId.New(), null, true, null, familyId, DateTimeOffset.UtcNow),
                ProcessingLogEntry.Create(FileId.New(), "doc.pdf", null, null, null, null, null, true, null, familyId, DateTimeOffset.UtcNow)
            ]);

        var query = new GetProcessingLogQuery()
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoEntries()
    {
        _logRepo.GetByFamilyIdAsync(FamilyId.New(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(new List<ProcessingLogEntry>());

        var query = new GetProcessingLogQuery()
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
        var familyId = FamilyId.New();
        _logRepo.GetByFamilyIdAsync(familyId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([
                ProcessingLogEntry.Create(FileId.New(), "mine.jpg", null, null, null, null, null, true, null, familyId, DateTimeOffset.UtcNow)
            ]);

        var query = new GetProcessingLogQuery()
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().FileName.Should().Be("mine.jpg");
    }
}
