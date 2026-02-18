using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record ProcessingLogEntryDto(
    Guid Id,
    Guid FileId,
    string FileName,
    Guid? MatchedRuleId,
    string? MatchedRuleName,
    RuleActionType? ActionTaken,
    Guid? DestinationFolderId,
    string? AppliedTagNames,
    bool Success,
    string? ErrorMessage,
    DateTime ProcessedAt);
