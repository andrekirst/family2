using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Models;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.RefreshGoogleToken;

public sealed record RefreshGoogleTokenCommand(
    UserId UserId
) : ICommand<RefreshTokenResultDto>;
