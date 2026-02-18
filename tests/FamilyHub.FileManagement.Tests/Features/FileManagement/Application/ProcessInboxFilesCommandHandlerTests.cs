using FamilyHub.Api.Features.FileManagement.Application.Commands.ProcessInboxFiles;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Services;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class ProcessInboxFilesCommandHandlerTests
{
    private readonly FamilyId _familyId = FamilyId.New();
    private readonly UserId _userId = UserId.New();

    private (ProcessInboxFilesCommandHandler handler,
        FakeFolderRepository folderRepo,
        FakeStoredFileRepository fileRepo,
        FakeOrganizationRuleRepository ruleRepo,
        FakeFileTagRepository tagRepo,
        FakeProcessingLogRepository logRepo) CreateHandler()
    {
        var folderRepo = new FakeFolderRepository();
        var fileRepo = new FakeStoredFileRepository();
        var ruleRepo = new FakeOrganizationRuleRepository();
        var tagRepo = new FakeFileTagRepository();
        var logRepo = new FakeProcessingLogRepository();
        var engine = new OrganizationRuleEngine();

        var handler = new ProcessInboxFilesCommandHandler(
            folderRepo, fileRepo, ruleRepo, tagRepo, logRepo, engine);

        return (handler, folderRepo, fileRepo, ruleRepo, tagRepo, logRepo);
    }

    private Folder CreateInbox()
    {
        var root = Folder.CreateRoot(_familyId, _userId);
        var inbox = Folder.CreateInbox(root.Id, _familyId, _userId);
        return inbox;
    }

    private StoredFile CreateFile(string name, string mimeType, FolderId folderId)
    {
        return StoredFile.Create(
            FileName.From(name),
            MimeType.From(mimeType),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            folderId,
            _familyId,
            _userId);
    }

    [Fact]
    public async Task Handle_ShouldProcessFilesAndMoveMatchingOnes()
    {
        var (handler, folderRepo, fileRepo, ruleRepo, _, logRepo) = CreateHandler();
        var inbox = CreateInbox();
        folderRepo.Folders.Add(inbox);

        var destFolderId = FolderId.New();
        var file = CreateFile("photo.jpg", "image/jpeg", inbox.Id);
        fileRepo.Files.Add(file);

        ruleRepo.Rules.Add(OrganizationRule.Create(
            "Move images", _familyId, _userId,
            """[{"Type":2,"Value":"image/*"}]""",
            ConditionLogic.And,
            RuleActionType.MoveToFolder,
            $$$"""{"DestinationFolderId":"{{{destFolderId.Value}}}"}""",
            1));

        var command = new ProcessInboxFilesCommand(_familyId, _userId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.FilesProcessed.Should().Be(1);
        result.RulesMatched.Should().Be(1);
        file.FolderId.Should().Be(destFolderId);
        logRepo.Entries.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldLogUnmatchedFiles()
    {
        var (handler, folderRepo, fileRepo, ruleRepo, _, logRepo) = CreateHandler();
        var inbox = CreateInbox();
        folderRepo.Folders.Add(inbox);

        var file = CreateFile("random.xyz", "application/octet-stream", inbox.Id);
        fileRepo.Files.Add(file);

        // Rule only matches .jpg
        ruleRepo.Rules.Add(OrganizationRule.Create(
            "Move photos", _familyId, _userId,
            """[{"Type":1,"Value":".jpg"}]""",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1));

        var command = new ProcessInboxFilesCommand(_familyId, _userId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.FilesProcessed.Should().Be(1);
        result.RulesMatched.Should().Be(0);
        logRepo.Entries.Should().HaveCount(1);
        logRepo.Entries.First().MatchedRuleId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldApplyTags()
    {
        var (handler, folderRepo, fileRepo, ruleRepo, tagRepo, _) = CreateHandler();
        var inbox = CreateInbox();
        folderRepo.Folders.Add(inbox);

        var file = CreateFile("photo.jpg", "image/jpeg", inbox.Id);
        fileRepo.Files.Add(file);

        var tagId = TagId.New();
        ruleRepo.Rules.Add(OrganizationRule.Create(
            "Tag photos", _familyId, _userId,
            """[{"Type":2,"Value":"image/*"}]""",
            ConditionLogic.And,
            RuleActionType.ApplyTags,
            $$$"""{"TagIds":["{{{tagId.Value}}}"]}""",
            1));

        var command = new ProcessInboxFilesCommand(_familyId, _userId);
        await handler.Handle(command, CancellationToken.None);

        tagRepo.FileTags.Should().HaveCount(1);
        tagRepo.FileTags.First().FileId.Should().Be(file.Id);
        tagRepo.FileTags.First().TagId.Should().Be(tagId);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenInboxNotFound()
    {
        var (handler, _, _, _, _, _) = CreateHandler();

        var command = new ProcessInboxFilesCommand(_familyId, _userId);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.InboxFolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoFiles()
    {
        var (handler, folderRepo, _, _, _, _) = CreateHandler();
        var inbox = CreateInbox();
        folderRepo.Folders.Add(inbox);

        var command = new ProcessInboxFilesCommand(_familyId, _userId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.FilesProcessed.Should().Be(0);
        result.RulesMatched.Should().Be(0);
    }
}
