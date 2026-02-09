using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands;

public sealed record SendInvitationResult(InvitationId InvitationId);
