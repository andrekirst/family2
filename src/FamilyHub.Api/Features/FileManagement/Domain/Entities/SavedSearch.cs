using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// A user-defined saved search with name and serialized filter state.
/// Allows users to quickly re-execute complex search queries.
/// </summary>
public sealed class SavedSearch
{
#pragma warning disable CS8618
    private SavedSearch() { }
#pragma warning restore CS8618

    public static SavedSearch Create(
        UserId userId,
        string name,
        string query,
        string? filtersJson)
    {
        return new SavedSearch
        {
            Id = SavedSearchId.New(),
            UserId = userId,
            Name = name,
            Query = query,
            FiltersJson = filtersJson,
            CreatedAt = DateTime.UtcNow
        };
    }

    public SavedSearchId Id { get; private set; }
    public UserId UserId { get; private set; }
    public string Name { get; private set; }
    public string Query { get; private set; }
    public string? FiltersJson { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void Rename(string newName)
    {
        Name = newName;
    }
}
