using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Domain.Events;

public sealed record MessageAttachmentAddedEvent(
    MessageId MessageId,
    FileId FileId,
    FamilyId FamilyId,
    UserId SenderId,
    DateTime AttachedAt
) : DomainEvent;
