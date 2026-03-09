using FamilyHub.Api.Features.FileManagement.Application.Commands.ProcessInboxFiles;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Services;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class ProcessInboxFilesCommandHandlerTests
{
    private readonly FamilyId _familyId = FamilyId.New();
    private readonly UserId _userId = UserId.New();

    private readonly IFolderRepository _folderRepo = Substitute.For<IFolderRepository>();
    private readonly IStoredFileRepository _fileRepo = Substitute.For<IStoredFileRepository>();
    private readonly IOrganizationRuleRepository _ruleRepo = Substitute.For<IOrganizationRuleRepository>();
    private readonly IProcessingLogRepository _logRepo = Substitute.For<IProcessingLogRepository>();
    private readonly IInboxFileProcessor _fileProcessor = Substitute.For<IInboxFileProcessor>();
    private readonly ProcessInboxFilesCommandHandler _handler;

    public ProcessInboxFilesCommandHandlerTests()
    {
        var engine = new OrganizationRuleEngine();
        var logger = NullLogger<ProcessInboxFilesCommandHandler>.Instance;
        _handler = new ProcessInboxFilesCommandHandler(
            _folderRepo, _fileRepo, _ruleRepo, _logRepo, engine, _fileProcessor, TimeProvider.System, logger);
    }

    private Folder CreateInbox()
    {
        var root = Folder.CreateRoot(_familyId, _userId, DateTimeOffset.UtcNow);
        var inbox = Folder.CreateInbox(root.Id, _familyId, _userId, DateTimeOffset.UtcNow);
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
            _userId, DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldProcessFilesAndMoveMatchingOnes()
    {
        var inbox = CreateInbox();
        _folderRepo.GetInboxFolderAsync(_familyId, Arg.Any<CancellationToken>()).Returns(inbox);

        var destFolderId = FolderId.New();
        var file = CreateFile("photo.jpg", "image/jpeg", inbox.Id);
        _fileRepo.GetByFolderIdAsync(inbox.Id, Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile> { file });

        var rule = OrganizationRule.Create(
            "Move images", _familyId, _userId,
            """[{"Type":2,"Value":"image/*"}]""",
            ConditionLogic.And,
            RuleActionType.MoveToFolder,
            $$$"""{"DestinationFolderId":"{{{destFolderId.Value}}}"}""",
            1, DateTimeOffset.UtcNow);
        _ruleRepo.GetEnabledByFamilyIdAsync(_familyId, Arg.Any<CancellationToken>())
            .Returns(new List<OrganizationRule> { rule });

        var logEntry = ProcessingLogEntry.Create(
            file.Id, "photo.jpg", rule.Id, "Move images",
            RuleActionType.MoveToFolder, destFolderId, null, true, null, _familyId, DateTimeOffset.UtcNow);
        _fileProcessor.ProcessFileAsync(file, Arg.Any<RuleMatchPreviewDto>(), _userId, _familyId, Arg.Any<CancellationToken>())
            .Returns(logEntry);

        var command = new ProcessInboxFilesCommand()
        {
            FamilyId = _familyId,
            UserId = _userId
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.FilesProcessed.Should().Be(1);
        result.RulesMatched.Should().Be(1);
        await _logRepo.Received(1).AddRangeAsync(
            Arg.Is<List<ProcessingLogEntry>>(entries => entries.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogUnmatchedFiles()
    {
        var inbox = CreateInbox();
        _folderRepo.GetInboxFolderAsync(_familyId, Arg.Any<CancellationToken>()).Returns(inbox);

        var file = CreateFile("random.xyz", "application/octet-stream", inbox.Id);
        _fileRepo.GetByFolderIdAsync(inbox.Id, Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile> { file });

        var rule = OrganizationRule.Create(
            "Move photos", _familyId, _userId,
            """[{"Type":1,"Value":".jpg"}]""",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1, DateTimeOffset.UtcNow);
        _ruleRepo.GetEnabledByFamilyIdAsync(_familyId, Arg.Any<CancellationToken>())
            .Returns(new List<OrganizationRule> { rule });

        var command = new ProcessInboxFilesCommand()
        {
            FamilyId = _familyId,
            UserId = _userId
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.FilesProcessed.Should().Be(1);
        result.RulesMatched.Should().Be(0);
        await _logRepo.Received(1).AddRangeAsync(
            Arg.Is<List<ProcessingLogEntry>>(entries => entries.Count == 1 && entries.First().MatchedRuleId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldApplyTags()
    {
        var inbox = CreateInbox();
        _folderRepo.GetInboxFolderAsync(_familyId, Arg.Any<CancellationToken>()).Returns(inbox);

        var file = CreateFile("photo.jpg", "image/jpeg", inbox.Id);
        _fileRepo.GetByFolderIdAsync(inbox.Id, Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile> { file });

        var tagId = TagId.New();
        var rule = OrganizationRule.Create(
            "Tag photos", _familyId, _userId,
            """[{"Type":2,"Value":"image/*"}]""",
            ConditionLogic.And,
            RuleActionType.ApplyTags,
            $$$"""{"TagIds":["{{{tagId.Value}}}"]}""",
            1, DateTimeOffset.UtcNow);
        _ruleRepo.GetEnabledByFamilyIdAsync(_familyId, Arg.Any<CancellationToken>())
            .Returns(new List<OrganizationRule> { rule });

        var logEntry = ProcessingLogEntry.Create(
            file.Id, "photo.jpg", rule.Id, "Tag photos",
            RuleActionType.ApplyTags, null, null, true, null, _familyId, DateTimeOffset.UtcNow);
        _fileProcessor.ProcessFileAsync(file, Arg.Any<RuleMatchPreviewDto>(), _userId, _familyId, Arg.Any<CancellationToken>())
            .Returns(logEntry);

        var command = new ProcessInboxFilesCommand()
        {
            FamilyId = _familyId,
            UserId = _userId
        };
        await _handler.Handle(command, CancellationToken.None);

        await _fileProcessor.Received(1).ProcessFileAsync(
            file, Arg.Any<RuleMatchPreviewDto>(), _userId, _familyId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenInboxNotFound()
    {
        _folderRepo.GetInboxFolderAsync(_familyId, Arg.Any<CancellationToken>())
            .Returns((Folder?)null);

        var command = new ProcessInboxFilesCommand()
        {
            FamilyId = _familyId,
            UserId = _userId
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.InboxFolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoFiles()
    {
        var inbox = CreateInbox();
        _folderRepo.GetInboxFolderAsync(_familyId, Arg.Any<CancellationToken>()).Returns(inbox);
        _fileRepo.GetByFolderIdAsync(inbox.Id, Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile>());

        var command = new ProcessInboxFilesCommand()
        {
            FamilyId = _familyId,
            UserId = _userId
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.FilesProcessed.Should().Be(0);
        result.RulesMatched.Should().Be(0);
    }
}
