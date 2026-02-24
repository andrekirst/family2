using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

/// <summary>
/// Raised when a family exceeds their storage quota.
/// </summary>
public sealed record StorageQuotaExceededEvent(
    FamilyId FamilyId,
    long UsedBytes,
    long MaxBytes) : DomainEvent;
