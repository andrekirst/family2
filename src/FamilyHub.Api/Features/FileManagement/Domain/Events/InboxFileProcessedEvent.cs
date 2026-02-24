using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

/// <summary>
/// Raised when a file in the inbox is auto-organized by a matching rule.
/// </summary>
public sealed record InboxFileProcessedEvent(
    FileId FileId,
    OrganizationRuleId RuleId,
    RuleActionType Action,
    FolderId? DestinationFolderId,
    FamilyId FamilyId) : DomainEvent;
