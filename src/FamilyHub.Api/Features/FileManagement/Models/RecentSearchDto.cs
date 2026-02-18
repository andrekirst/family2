namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record RecentSearchDto(Guid Id, string Query, DateTime SearchedAt);
