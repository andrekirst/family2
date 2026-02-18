using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

public sealed record ShareLinkAccessedEvent(
    ShareLinkId ShareLinkId,
    Guid ResourceId,
    string IpAddress,
    ShareAccessAction Action) : DomainEvent;
