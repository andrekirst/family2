namespace FamilyHub.Api.Features.Family.Application.Queries.GetAvatar;

public sealed record GetAvatarResult(
    Stream Data,
    string MimeType,
    string ETag);
