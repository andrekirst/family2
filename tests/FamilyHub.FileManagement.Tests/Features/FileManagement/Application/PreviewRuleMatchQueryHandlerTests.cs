using FamilyHub.Api.Features.FileManagement.Application.Queries.PreviewRuleMatch;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Services;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class PreviewRuleMatchQueryHandlerTests
{
    private readonly FamilyId _familyId = FamilyId.New();
    private readonly UserId _userId = UserId.New();

    [Fact]
    public async Task Handle_ShouldReturnMatchingRule()
    {
        var fileRepo = new FakeStoredFileRepository();
        var ruleRepo = new FakeOrganizationRuleRepository();
        var engine = new OrganizationRuleEngine();
        var handler = new PreviewRuleMatchQueryHandler(fileRepo, ruleRepo, engine);

        var file = StoredFile.Create(
            FileName.From("photo.jpg"), MimeType.From("image/jpeg"),
            FileSize.From(1024), StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(), _familyId, _userId);
        fileRepo.Files.Add(file);

        ruleRepo.Rules.Add(OrganizationRule.Create(
            "Photos rule", _familyId, _userId,
            """[{"Type":2,"Value":"image/*"}]""",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1));

        var query = new PreviewRuleMatchQuery(file.Id, _familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Matched.Should().BeTrue();
        result.MatchedRuleName.Should().Be("Photos rule");
    }

    [Fact]
    public async Task Handle_ShouldReturnNullWhenNoMatch()
    {
        var fileRepo = new FakeStoredFileRepository();
        var ruleRepo = new FakeOrganizationRuleRepository();
        var engine = new OrganizationRuleEngine();
        var handler = new PreviewRuleMatchQueryHandler(fileRepo, ruleRepo, engine);

        var file = StoredFile.Create(
            FileName.From("doc.txt"), MimeType.From("text/plain"),
            FileSize.From(100), StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(), _familyId, _userId);
        fileRepo.Files.Add(file);

        ruleRepo.Rules.Add(OrganizationRule.Create(
            "Photos only", _familyId, _userId,
            """[{"Type":2,"Value":"image/*"}]""",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1));

        var query = new PreviewRuleMatchQuery(file.Id, _familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        var fileRepo = new FakeStoredFileRepository();
        var ruleRepo = new FakeOrganizationRuleRepository();
        var engine = new OrganizationRuleEngine();
        var handler = new PreviewRuleMatchQueryHandler(fileRepo, ruleRepo, engine);

        var query = new PreviewRuleMatchQuery(FileId.New(), _familyId);
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FileNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileBelongsToDifferentFamily()
    {
        var fileRepo = new FakeStoredFileRepository();
        var ruleRepo = new FakeOrganizationRuleRepository();
        var engine = new OrganizationRuleEngine();
        var handler = new PreviewRuleMatchQueryHandler(fileRepo, ruleRepo, engine);

        var file = StoredFile.Create(
            FileName.From("photo.jpg"), MimeType.From("image/jpeg"),
            FileSize.From(1024), StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(), FamilyId.New(), _userId); // Different family
        fileRepo.Files.Add(file);

        var query = new PreviewRuleMatchQuery(file.Id, _familyId);
        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
