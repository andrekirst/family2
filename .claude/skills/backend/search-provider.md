---
name: search-provider
file: backend/search-provider.md
description: Create search provider and command palette provider for a module
category: backend
module-aware: true
inputs:
  - module: DDD module name
  - hasSearchableData: Whether module has queryable entities (default: true)
  - hasCommands: Whether module provides command palette items (default: true)
---

# Search Provider Skill

Create `ISearchProvider` and `ICommandPaletteProvider` implementations for a module.

## Search Provider

Location: `Features/{Module}/Application/Search/{Module}SearchProvider.cs`

```csharp
public sealed class {Module}SearchProvider(AppDbContext db) : ISearchProvider
{
    public string ModuleName => "{module}";

    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(context.Query))
            return [];

        var normalizedQuery = context.Query.Trim().ToLowerInvariant();

        var results = await db.{Entities}
            .Where(e => e.FamilyId == context.FamilyId
                && EF.Functions.ILike(e.Name, $"%{normalizedQuery}%"))
            .Take(context.MaxResults)
            .Select(e => new SearchResultItem
            {
                Id = e.Id.ToString(),
                Title = e.Name,
                Module = "{module}",
                Type = "{entity}",
                Url = $"/{module}/{e.Id}",
                Relevance = 1.0
            })
            .ToListAsync(ct);

        return results;
    }
}
```

## Command Palette Provider

Location: `Features/{Module}/Application/Search/{Module}CommandPaletteProvider.cs`

```csharp
public sealed class {Module}CommandPaletteProvider : ICommandPaletteProvider
{
    public string ModuleName => "{module}";

    public IReadOnlyList<CommandDescriptor> GetCommands() =>
    [
        new()
        {
            Id = "{module}.create",
            Label = "Create {Entity}",
            LabelDe = "{Entity} erstellen",
            Description = "Create a new {entity}",
            DescriptionDe = "Neues {Entity} erstellen",
            Icon = "add",
            Module = "{module}",
            Route = "/{module}/create",
            Keywords = ["create", "new", "{entity}"]
        }
    ];
}
```

## Module Registration

Add to the module's `RegisterServices()`:

```csharp
// Search integration
services.AddScoped<ISearchProvider, {Module}SearchProvider>();
services.AddSingleton<ICommandPaletteProvider, {Module}CommandPaletteProvider>();
```

## Test Pattern

```csharp
using FamilyHub.TestCommon.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

[Fact]
public async Task SearchAsync_ShouldReturnResults_WhenQueryMatches()
{
    var provider = new FakeSearchProvider("{module}", [
        new SearchResultItem { Id = "1", Title = "Test", Module = "{module}" }
    ]);

    var context = new SearchContext { Query = "test", FamilyId = familyId };
    var results = await provider.SearchAsync(context, CancellationToken.None);

    results.Should().HaveCount(1);
    provider.SearchCallCount.Should().Be(1);
}
```

## Validation

- [ ] SearchProvider is registered as **scoped** (not singleton)
- [ ] CommandPaletteProvider is registered as **singleton**
- [ ] SearchProvider filters by FamilyId (multi-tenancy)
- [ ] CommandDescriptor includes i18n fields (LabelDe, DescriptionDe)
- [ ] Module folder: `Application/Search/` contains both files
- [ ] Tests use `FakeSearchProvider` from TestCommon
