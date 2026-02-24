using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.Events;

public sealed record GoogleAccountLinkedEvent(
    GoogleAccountLinkId LinkId,
    UserId UserId,
    GoogleAccountId GoogleAccountId,
    GoogleScopes GrantedScopes
) : DomainEvent;
