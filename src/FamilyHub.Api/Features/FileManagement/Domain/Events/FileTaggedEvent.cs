using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

public sealed record FileTaggedEvent(
    FileId FileId,
    TagId TagId,
    TagName TagName,
    FamilyId FamilyId) : DomainEvent;
