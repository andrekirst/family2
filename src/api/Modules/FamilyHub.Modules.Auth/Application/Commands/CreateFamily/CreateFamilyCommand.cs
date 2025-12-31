using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Commands.CreateFamily;

/// <summary>
/// Command to create a new family with the authenticated user as owner.
/// Authentication is extracted by the handler via ICurrentUserService.
/// </summary>
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>;
