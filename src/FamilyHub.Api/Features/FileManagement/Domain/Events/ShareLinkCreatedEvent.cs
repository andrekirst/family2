using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

public sealed record ShareLinkCreatedEvent(
    ShareLinkId ShareLinkId,
    Guid ResourceId,
    FamilyId FamilyId,
    UserId CreatedBy,
    DateTime? ExpiresAt) : DomainEvent;
