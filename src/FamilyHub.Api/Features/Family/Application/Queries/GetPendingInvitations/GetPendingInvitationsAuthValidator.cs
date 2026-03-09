using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetPendingInvitations;

/// <summary>
/// Authorization validator for GetPendingInvitationsQuery.
/// Checks that the requesting user has permission to view invitations for the family.
/// </summary>
public sealed class GetPendingInvitationsAuthValidator : AbstractValidator<GetPendingInvitationsQuery>, IAuthValidator<GetPendingInvitationsQuery>
{
    public GetPendingInvitationsAuthValidator(
        FamilyAuthorizationService authService,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (query, cancellationToken) =>
            {
                return await authService.CanInviteAsync(query.UserId, query.FamilyId, cancellationToken);
            })
            .WithErrorCode(DomainErrorCodes.InsufficientPermissionToViewInvitations)
            .WithMessage(_ => localizer[DomainErrorCodes.InsufficientPermissionToViewInvitations].Value);
    }
}
