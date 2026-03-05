# Search Provider Pattern

Universal search uses per-module providers registered via DI.

## ISearchProvider (Scoped)

Each module implements `ISearchProvider` in `Application/Search/`:

```csharp
public sealed class FamilySearchProvider(AppDbContext db) : ISearchProvider
{
    public string ModuleName => "family";
    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context, CancellationToken ct)
    {
        // Query PostgreSQL, return SearchResultItem list
    }
}
```

Registration: `services.AddScoped<ISearchProvider, FamilySearchProvider>();`

## ICommandPaletteProvider (Singleton)

Modules provide static command descriptors:

```csharp
public sealed class FamilyCommandPaletteProvider : ICommandPaletteProvider
{
    public string ModuleName => "family";
    public IReadOnlyList<CommandDescriptor> GetCommands() => [ ... ];
}
```

## CommandPaletteRegistry

Singleton cache populated at startup via `IHostedService`:

```csharp
services.AddSingleton<ICommandPaletteRegistry, CommandPaletteRegistry>();
services.AddSingleton<ICommandPaletteProvider, FamilyCommandPaletteProvider>();
services.AddHostedService<CommandPaletteInitializer>();
```

## Cross-Cutting Infrastructure

Shared interfaces live in `Common/Search/`:

- `ISearchProvider`, `ICommandPaletteProvider`, `ICommandPaletteRegistry`
- `SearchContext`, `SearchResultItem`, `CommandDescriptor`

## Per-Module Layout

```
Features/{Module}/Application/Search/
├── {Module}SearchProvider.cs
└── {Module}CommandPaletteProvider.cs
```

## Rules

- Search providers are **scoped** (share DbContext per request)
- Command providers are **singletons** (static data)
- Sequential execution (not parallel) — providers share scoped DbContext
- Overall result cap: 30 items
- CommandDescriptor supports i18n: LabelDe, DescriptionDe optional fields
