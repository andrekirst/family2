using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.Shared;

public sealed record AcceptInvitationResult(FamilyId FamilyId, FamilyMemberId FamilyMemberId);
