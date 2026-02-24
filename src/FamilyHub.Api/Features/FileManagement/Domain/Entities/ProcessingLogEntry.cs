using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Records the result of auto-organization rule processing for a single file.
/// Provides an audit trail showing what rule matched and what action was taken.
/// </summary>
public sealed class ProcessingLogEntry
{
#pragma warning disable CS8618
    private ProcessingLogEntry() { }
#pragma warning restore CS8618

    public static ProcessingLogEntry Create(
        FileId fileId,
        string fileName,
        OrganizationRuleId? matchedRuleId,
        string? matchedRuleName,
        RuleActionType? actionTaken,
        FolderId? destinationFolderId,
        string? appliedTagNames,
        bool success,
        string? errorMessage,
        FamilyId familyId)
    {
        return new ProcessingLogEntry
        {
            Id = ProcessingLogEntryId.New(),
            FileId = fileId,
            FileName = fileName,
            MatchedRuleId = matchedRuleId,
            MatchedRuleName = matchedRuleName,
            ActionTaken = actionTaken,
            DestinationFolderId = destinationFolderId,
            AppliedTagNames = appliedTagNames,
            Success = success,
            ErrorMessage = errorMessage,
            FamilyId = familyId,
            ProcessedAt = DateTime.UtcNow
        };
    }

    public ProcessingLogEntryId Id { get; private set; }
    public FileId FileId { get; private set; }
    public string FileName { get; private set; }
    public OrganizationRuleId? MatchedRuleId { get; private set; }
    public string? MatchedRuleName { get; private set; }
    public RuleActionType? ActionTaken { get; private set; }
    public FolderId? DestinationFolderId { get; private set; }
    public string? AppliedTagNames { get; private set; }
    public bool Success { get; private set; }
    public string? ErrorMessage { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public DateTime ProcessedAt { get; private set; }
}
