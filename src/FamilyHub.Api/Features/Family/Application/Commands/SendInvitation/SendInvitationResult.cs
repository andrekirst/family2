using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;

public sealed record SendInvitationResult(InvitationId InvitationId);
