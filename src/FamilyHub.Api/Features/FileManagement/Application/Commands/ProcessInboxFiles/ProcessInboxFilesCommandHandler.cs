using System.Text.Json;
using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ProcessInboxFiles;

public sealed class ProcessInboxFilesCommandHandler(
    IFolderRepository folderRepository,
    IStoredFileRepository fileRepository,
    IOrganizationRuleRepository ruleRepository,
    IFileTagRepository fileTagRepository,
    IProcessingLogRepository logRepository,
    IOrganizationRuleEngine ruleEngine)
    : ICommandHandler<ProcessInboxFilesCommand, ProcessInboxFilesResult>
{
    public async ValueTask<ProcessInboxFilesResult> Handle(
        ProcessInboxFilesCommand command,
        CancellationToken cancellationToken)
    {
        var inboxFolder = await folderRepository.GetInboxFolderAsync(command.FamilyId, cancellationToken)
            ?? throw new DomainException("Inbox folder not found", DomainErrorCodes.InboxFolderNotFound);

        var files = await fileRepository.GetByFolderIdAsync(inboxFolder.Id, cancellationToken);
        var rules = await ruleRepository.GetEnabledByFamilyIdAsync(command.FamilyId, cancellationToken);

        var logEntries = new List<ProcessingLogEntry>();
        var rulesMatched = 0;

        foreach (var file in files)
        {
            var match = ruleEngine.EvaluateFile(file, rules);

            if (match is { Matched: true })
            {
                rulesMatched++;
                var logEntry = await ApplyRuleAction(file, match, command.UserId, command.FamilyId, cancellationToken);
                logEntries.Add(logEntry);
            }
            else
            {
                // No rule matched — log as unmatched
                var logEntry = ProcessingLogEntry.Create(
                    file.Id, file.Name.Value,
                    null, null, null, null, null,
                    true, null, command.FamilyId);
                logEntries.Add(logEntry);
            }
        }

        if (logEntries.Count > 0)
            await logRepository.AddRangeAsync(logEntries, cancellationToken);

        return new ProcessInboxFilesResult(
            true,
            files.Count,
            rulesMatched,
            logEntries.Select(FileManagementMapper.ToDto).ToList());
    }

    private async Task<ProcessingLogEntry> ApplyRuleAction(
        StoredFile file,
        RuleMatchPreviewDto match,
        UserId movedBy,
        FamilyId familyId,
        CancellationToken ct)
    {
        try
        {
            RuleActionDto? action = null;
            if (match.ActionsJson is not null)
            {
                action = JsonSerializer.Deserialize<RuleActionDto>(match.ActionsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            FolderId? destinationFolderId = null;
            string? appliedTagNames = null;

            if (action is not null)
            {
                // Move to folder action
                if (match.ActionType is RuleActionType.MoveToFolder or RuleActionType.MoveAndTag
                    && action.DestinationFolderId.HasValue)
                {
                    destinationFolderId = FolderId.From(action.DestinationFolderId.Value);
                    file.MoveTo(destinationFolderId.Value, movedBy);
                }

                // Apply tags action
                if (match.ActionType is RuleActionType.ApplyTags or RuleActionType.MoveAndTag
                    && action.TagIds is { Count: > 0 })
                {
                    var tagNames = new List<string>();
                    foreach (var tagId in action.TagIds)
                    {
                        var fileTag = FileTag.Create(file.Id, TagId.From(tagId));
                        await fileTagRepository.AddAsync(fileTag, ct);
                        tagNames.Add(tagId.ToString());
                    }
                    appliedTagNames = string.Join(", ", tagNames);
                }
            }

            return ProcessingLogEntry.Create(
                file.Id, file.Name.Value,
                match.MatchedRuleId.HasValue ? OrganizationRuleId.From(match.MatchedRuleId.Value) : null,
                match.MatchedRuleName,
                match.ActionType,
                destinationFolderId,
                appliedTagNames,
                true, null, familyId);
        }
        catch (Exception ex)
        {
            return ProcessingLogEntry.Create(
                file.Id, file.Name.Value,
                match.MatchedRuleId.HasValue ? OrganizationRuleId.From(match.MatchedRuleId.Value) : null,
                match.MatchedRuleName,
                match.ActionType,
                null, null,
                false, ex.Message, familyId);
        }
    }
}
