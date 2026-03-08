using System.Text.Json;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ProcessInboxFiles;

/// <summary>
/// Processes a single inbox file by applying the matched rule action (move, tag, or both).
/// This service contains no error handling — exceptions propagate to the orchestrator.
/// </summary>
public sealed class InboxFileProcessor(
    IFileTagRepository fileTagRepository) : IInboxFileProcessor
{
    public async Task<ProcessingLogEntry> ProcessFileAsync(
        StoredFile file,
        RuleMatchPreviewDto match,
        UserId movedBy,
        FamilyId familyId,
        CancellationToken cancellationToken)
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
            if (match.ActionType is RuleActionType.MoveToFolder or RuleActionType.MoveAndTag
                && action.DestinationFolderId.HasValue)
            {
                destinationFolderId = FolderId.From(action.DestinationFolderId.Value);
                file.MoveTo(destinationFolderId.Value, movedBy);
            }

            if (match.ActionType is RuleActionType.ApplyTags or RuleActionType.MoveAndTag
                && action.TagIds is { Count: > 0 })
            {
                var tagNames = new List<string>();
                foreach (var tagId in action.TagIds)
                {
                    var fileTag = FileTag.Create(file.Id, TagId.From(tagId));
                    await fileTagRepository.AddAsync(fileTag, cancellationToken);
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
}
