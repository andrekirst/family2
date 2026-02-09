using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands;

public sealed record AcceptInvitationResult(FamilyId FamilyId, FamilyMemberId FamilyMemberId);
