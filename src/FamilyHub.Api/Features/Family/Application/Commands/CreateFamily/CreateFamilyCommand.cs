using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.CreateFamily;

/// <summary>
/// Command to create a new family with the specified owner.
/// </summary>
public sealed record CreateFamilyCommand(
    FamilyName Name,
    UserId OwnerId
) : ICommand<CreateFamilyResult>;
