using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain;

public class ProcessingLogEntryTests
{
    [Fact]
    public void Create_ShouldInitializeSuccessEntry()
    {
        var fileId = FileId.New();
        var ruleId = OrganizationRuleId.New();
        var folderId = FolderId.New();
        var familyId = FamilyId.New();

        var entry = ProcessingLogEntry.Create(
            fileId, "photo.jpg",
            ruleId, "Move photos",
            RuleActionType.MoveToFolder,
            folderId, null,
            true, null, familyId);

        entry.FileId.Should().Be(fileId);
        entry.FileName.Should().Be("photo.jpg");
        entry.MatchedRuleId.Should().Be(ruleId);
        entry.MatchedRuleName.Should().Be("Move photos");
        entry.ActionTaken.Should().Be(RuleActionType.MoveToFolder);
        entry.DestinationFolderId.Should().Be(folderId);
        entry.Success.Should().BeTrue();
        entry.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldInitializeFailedEntry()
    {
        var entry = ProcessingLogEntry.Create(
            FileId.New(), "bad-file.txt",
            OrganizationRuleId.New(), "Rule",
            RuleActionType.MoveToFolder,
            null, null,
            false, "Folder not found", FamilyId.New());

        entry.Success.Should().BeFalse();
        entry.ErrorMessage.Should().Be("Folder not found");
    }

    [Fact]
    public void Create_ShouldInitializeUnmatchedEntry()
    {
        var entry = ProcessingLogEntry.Create(
            FileId.New(), "random.txt",
            null, null, null, null, null,
            true, null, FamilyId.New());

        entry.MatchedRuleId.Should().BeNull();
        entry.MatchedRuleName.Should().BeNull();
        entry.ActionTaken.Should().BeNull();
        entry.Success.Should().BeTrue();
    }
}
