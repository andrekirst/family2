using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Commands.InviteFamilyMemberByEmail;

/// <summary>
/// Command to invite a family member by email.
/// </summary>
/// <param name="FamilyId">The ID of the family to invite to.</param>
/// <param name="Email">The email address of the person to invite.</param>
/// <param name="Role">The role to assign to the invited member.</param>
/// <param name="Message">Optional personal message for the invitation.</param>
public record InviteFamilyMemberByEmailCommand(
    FamilyId FamilyId,
    Email Email,
    UserRole Role,
    string? Message = null
) : IRequest<FamilyHub.SharedKernel.Domain.Result<InviteFamilyMemberByEmailResult>>;
