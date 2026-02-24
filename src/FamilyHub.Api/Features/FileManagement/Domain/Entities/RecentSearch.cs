using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Tracks a user's recent search queries (last 10 per user).
/// User-scoped, not family-scoped.
/// </summary>
public sealed class RecentSearch
{
#pragma warning disable CS8618
    private RecentSearch() { }
#pragma warning restore CS8618

    public static RecentSearch Create(UserId userId, string query)
    {
        return new RecentSearch
        {
            Id = RecentSearchId.New(),
            UserId = userId,
            Query = query,
            SearchedAt = DateTime.UtcNow
        };
    }

    public RecentSearchId Id { get; private set; }
    public UserId UserId { get; private set; }
    public string Query { get; private set; }
    public DateTime SearchedAt { get; private set; }
}
