using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.CreateFamily;

/// <summary>
/// Command to create a new family with the authenticated user as owner.
/// User context is automatically provided by UserContextEnrichmentBehavior.
/// </summary>
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>,
    IRequireAuthentication;
