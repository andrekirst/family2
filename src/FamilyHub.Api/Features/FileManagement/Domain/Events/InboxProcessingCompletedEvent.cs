using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

/// <summary>
/// Raised when batch inbox processing completes for a family.
/// </summary>
public sealed record InboxProcessingCompletedEvent(
    FamilyId FamilyId,
    int FilesProcessed,
    int RulesMatched) : DomainEvent;
