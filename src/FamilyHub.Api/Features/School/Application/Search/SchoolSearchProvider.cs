using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Search;

public sealed class SchoolSearchProvider(
    IStudentRepository studentRepository,
    IFamilyMemberRepository familyMemberRepository) : ISearchProvider
{
    public string ModuleName => "school";

    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.FamilyId is null || string.IsNullOrWhiteSpace(context.Query))
        {
            return [];
        }

        var students = await studentRepository.GetByFamilyIdAsync(context.FamilyId.Value, cancellationToken);
        if (students.Count == 0)
        {
            return [];
        }

        var members = await familyMemberRepository.GetByFamilyIdAsync(context.FamilyId.Value, cancellationToken);
        var nameLookup = members.ToDictionary(m => m.Id, m => m.User?.Name.Value ?? "");

        var query = context.Query.Trim();
        var queryWords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var tier1 = new List<(string Name, DateTime MarkedAt)>();
        var tier2 = new List<(string Name, DateTime MarkedAt, int Distance)>();

        foreach (var student in students)
        {
            if (!nameLookup.TryGetValue(student.FamilyMemberId, out var name) || string.IsNullOrEmpty(name))
            {
                continue;
            }

            // Tier 1: partial/substring match
            if (name.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                tier1.Add((name, student.MarkedAt));
                continue;
            }

            // Tier 2: word-level Levenshtein (typo tolerance)
            var nameWords = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var bestDistance = int.MaxValue;

            foreach (var qw in queryWords)
            {
                var maxAllowed = Math.Max(1, qw.Length / 4);
                foreach (var nw in nameWords)
                {
                    var dist = LevenshteinDistance(qw.ToLowerInvariant(), nw.ToLowerInvariant());
                    if (dist <= maxAllowed && dist < bestDistance)
                    {
                        bestDistance = dist;
                    }
                }
            }

            if (bestDistance < int.MaxValue)
            {
                tier2.Add((name, student.MarkedAt, bestDistance));
            }
        }

        var isGerman = context.Locale?.StartsWith("de", StringComparison.OrdinalIgnoreCase) == true;
        var sinceLabel = isGerman ? "Schüler seit" : "Student since";

        var results = tier1
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Select(t => ToSearchResult(t.Name, t.MarkedAt, sinceLabel))
            .Concat(tier2
                .OrderBy(t => t.Distance)
                .ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .Select(t => ToSearchResult(t.Name, t.MarkedAt, sinceLabel)))
            .Take(context.Limit)
            .ToList()
            .AsReadOnly();

        return results;
    }

    private static SearchResultItem ToSearchResult(string name, DateTime markedAt, string sinceLabel) =>
        new(
            Title: name,
            Description: $"{sinceLabel} {markedAt:yyyy-MM-dd}",
            Module: "school",
            Icon: "graduation-cap",
            Route: "/school");

    private static int LevenshteinDistance(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            prev[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(
                    Math.Min(curr[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost);
            }

            (prev, curr) = (curr, prev);
        }

        return prev[b.Length];
    }
}
