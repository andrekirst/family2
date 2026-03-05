using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Search;

public sealed class FamilySearchProvider(
    IFamilyMemberRepository familyMemberRepository,
    IFamilyRepository familyRepository,
    IFamilyInvitationRepository familyInvitationRepository) : ISearchProvider
{
    public string ModuleName => "family";

    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.FamilyId is null)
        {
            return [];
        }

        var queryLower = context.Query.ToLowerInvariant();
        var isGerman = context.Locale?.StartsWith("de", StringComparison.OrdinalIgnoreCase) == true;
        var results = new List<SearchResultItem>();

        // Search family name
        var family = await familyRepository.GetByIdAsync(context.FamilyId.Value, cancellationToken);
        if (family is not null &&
            family.Name.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
        {
            results.Add(new SearchResultItem(
                Title: family.Name.Value,
                Description: isGerman ? "Deine Familie" : "Your family",
                Module: "family",
                Icon: "home",
                Route: "/family"));
        }

        // Search family members
        var members = await familyMemberRepository.GetByFamilyIdAsync(
            context.FamilyId.Value, cancellationToken);

        var memberResults = members
            .Where(m => m.User?.Name.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase) == true ||
                        m.User?.Email.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase) == true)
            .Select(m => new SearchResultItem(
                Title: m.User?.Name.Value ?? "Unknown",
                Description: m.Role.Value,
                Module: "family",
                Icon: "users",
                Route: "/family"));

        results.AddRange(memberResults);

        // Search pending invitations
        var invitations = await familyInvitationRepository.GetPendingByFamilyIdAsync(
            context.FamilyId.Value, cancellationToken);

        var invitationResults = invitations
            .Where(i => i.InviteeEmail.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
            .Select(i => new SearchResultItem(
                Title: i.InviteeEmail.Value,
                Description: isGerman ? "Ausstehende Einladung" : "Pending invitation",
                Module: "family",
                Icon: "mail",
                Route: "/family"));

        results.AddRange(invitationResults);

        return results
            .Take(context.Limit)
            .ToList()
            .AsReadOnly();
    }
}
