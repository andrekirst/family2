namespace FamilyHub.Api.Features.GoogleIntegration.Models;

public sealed record RefreshTokenResultDto(bool Success, DateTime? NewExpiresAt);
