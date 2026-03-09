using FamilyHub.Api.Features.FileManagement.Application.Queries.PreviewRuleMatch;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Services;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class PreviewRuleMatchQueryHandlerTests
{
    private readonly FamilyId _familyId = FamilyId.New();
    private readonly UserId _userId = UserId.New();

    [Fact]
    public async Task Handle_ShouldReturnMatchingRule()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var ruleRepo = Substitute.For<IOrganizationRuleRepository>();
        var engine = new OrganizationRuleEngine();
        var handler = new PreviewRuleMatchQueryHandler(fileRepo, ruleRepo, engine);

        var file = StoredFile.Create(
            FileName.From("photo.jpg"), MimeType.From("image/jpeg"),
            FileSize.From(1024), StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(), _familyId, _userId, DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var rule = OrganizationRule.Create(
            "Photos rule", _familyId, _userId,
            """[{"Type":2,"Value":"image/*"}]""",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1, DateTimeOffset.UtcNow);
        ruleRepo.GetEnabledByFamilyIdAsync(_familyId, Arg.Any<CancellationToken>())
            .Returns(new List<OrganizationRule> { rule });

        var query = new PreviewRuleMatchQuery(file.Id)
        {
            FamilyId = _familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Matched.Should().BeTrue();
        result.Value.MatchedRuleName.Should().Be("Photos rule");
    }

    [Fact]
    public async Task Handle_ShouldReturnNullWhenNoMatch()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var ruleRepo = Substitute.For<IOrganizationRuleRepository>();
        var engine = new OrganizationRuleEngine();
        var handler = new PreviewRuleMatchQueryHandler(fileRepo, ruleRepo, engine);

        var file = StoredFile.Create(
            FileName.From("doc.txt"), MimeType.From("text/plain"),
            FileSize.From(100), StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(), _familyId, _userId, DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var rule = OrganizationRule.Create(
            "Photos only", _familyId, _userId,
            """[{"Type":2,"Value":"image/*"}]""",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1, DateTimeOffset.UtcNow);
        ruleRepo.GetEnabledByFamilyIdAsync(_familyId, Arg.Any<CancellationToken>())
            .Returns(new List<OrganizationRule> { rule });

        var query = new PreviewRuleMatchQuery(file.Id)
        {
            FamilyId = _familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var ruleRepo = Substitute.For<IOrganizationRuleRepository>();
        var engine = new OrganizationRuleEngine();
        var handler = new PreviewRuleMatchQueryHandler(fileRepo, ruleRepo, engine);

        fileRepo.GetByIdAsync(FileId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((StoredFile?)null);

        var query = new PreviewRuleMatchQuery(FileId.New())
        {
            FamilyId = _familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.FileNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileBelongsToDifferentFamily()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var ruleRepo = Substitute.For<IOrganizationRuleRepository>();
        var engine = new OrganizationRuleEngine();
        var handler = new PreviewRuleMatchQueryHandler(fileRepo, ruleRepo, engine);

        var file = StoredFile.Create(
            FileName.From("photo.jpg"), MimeType.From("image/jpeg"),
            FileSize.From(1024), StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(), FamilyId.New(), _userId, DateTimeOffset.UtcNow); // Different family
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var query = new PreviewRuleMatchQuery(file.Id)
        {
            FamilyId = _familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
