using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

public sealed record FileUntaggedEvent(
    FileId FileId,
    TagId TagId,
    FamilyId FamilyId) : DomainEvent;
