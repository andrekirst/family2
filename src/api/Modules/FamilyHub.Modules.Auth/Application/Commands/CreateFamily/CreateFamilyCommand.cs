using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Commands.CreateFamily;

/// <summary>
/// Command to create a new family with the specified user as owner.
/// </summary>
public sealed record CreateFamilyCommand(
    string Name,
    UserId UserId
) : IRequest<CreateFamilyResult>;
