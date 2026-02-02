using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands;

/// <summary>
/// Command to add a member to an existing family.
/// </summary>
public sealed record AddFamilyMemberCommand(
    FamilyId FamilyId,
    UserId UserIdToAdd
) : ICommand<bool>;
