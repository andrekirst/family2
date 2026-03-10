using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.UnlinkGoogleAccount;

public sealed record UnlinkGoogleAccountCommand : ICommand<bool>, IRequireUser
{
    public UserId UserId { get; init; }
}
