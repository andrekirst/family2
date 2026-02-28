using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Photos.Domain.Events;

public sealed record PhotoDeletedEvent(
    PhotoId PhotoId,
    FamilyId FamilyId,
    UserId DeletedBy,
    DateTime DeletedAt
) : DomainEvent;
