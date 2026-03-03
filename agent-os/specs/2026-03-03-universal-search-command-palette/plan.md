# Universal Search and Command Palette

**Created**: 2026-03-03
**GitHub Issue**: #208
**Spec**: `agent-os/specs/2026-03-03-universal-search-command-palette/`

## Context

Family Hub needs a unified way for users to search across all modules (calendar, family, messaging, files) and quickly execute common actions. Currently there's no search functionality and users must navigate manually to each module. This feature combines three capabilities:

1. **Universal Search** — Search across all modules, scoped to user's family/permissions
2. **Command Palette** — Quick actions like "create event" with navigation and form prefilling
3. **Natural Language Input** — Rule-based pattern matching (e.g., "Morgen Termin um 10 Uhr" → create calendar event)

## Architecture Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Module boundary | Hybrid: `Common/Search/` interfaces + `Features/Search/` module | Mirrors the proven Widget pattern (`Common/Widgets/` + `Features/Dashboard/`) |
| Search strategy | Real-time query fanout via `ISearchProvider` | Family-scale data (tens-hundreds of records) doesn't need a dedicated index |
| Command registry | Static `ICommandPaletteProvider` returning `CommandDescriptor[]` | Commands are metadata-only (label, route, permissions, keywords) |
| NLP engine | Frontend (TypeScript), rule-based pattern matching | Zero-latency parsing, no server round-trip, backend only sees structured queries |
| GraphQL API | `SearchQuery` namespace type under `RootQuery` | Consistent with `FamilyQuery`, `MessagingQuery`, `DashboardQuery` |

## Files to Create

### Backend — Common/Search/ (~10 files)

- `src/FamilyHub.Api/Common/Search/ISearchProvider.cs`
- `src/FamilyHub.Api/Common/Search/ICommandPaletteProvider.cs`
- `src/FamilyHub.Api/Common/Search/ISearchRegistry.cs`
- `src/FamilyHub.Api/Common/Search/ICommandPaletteRegistry.cs`
- `src/FamilyHub.Api/Common/Search/SearchRegistry.cs`
- `src/FamilyHub.Api/Common/Search/CommandPaletteRegistry.cs`
- `src/FamilyHub.Api/Common/Search/SearchRegistryInitializer.cs`
- `src/FamilyHub.Api/Common/Search/SearchContext.cs`
- `src/FamilyHub.Api/Common/Search/SearchResultItem.cs`
- `src/FamilyHub.Api/Common/Search/CommandDescriptor.cs`

### Backend — Features/Search/ (~8 files)

- `src/FamilyHub.Api/Features/Search/SearchModule.cs`
- `src/FamilyHub.Api/Features/Search/Application/Queries/UniversalSearch/UniversalSearchQuery.cs`
- `src/FamilyHub.Api/Features/Search/Application/Queries/UniversalSearch/UniversalSearchQueryHandler.cs`
- `src/FamilyHub.Api/Features/Search/Application/Queries/UniversalSearch/UniversalSearchResult.cs`
- `src/FamilyHub.Api/Features/Search/Application/Queries/UniversalSearch/QueryType.cs`
- `src/FamilyHub.Api/Features/Search/Models/UniversalSearchRequest.cs`
- `src/FamilyHub.Api/Features/Search/Models/SearchResultItemDto.cs`
- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/SearchQuery.cs`

### Backend — Module Search Providers (~4 files initially)

- `Features/Family/Application/Search/FamilySearchProvider.cs`
- `Features/Family/Application/Search/FamilyCommandPaletteProvider.cs`
- `Features/Calendar/Application/Search/CalendarSearchProvider.cs`
- `Features/Calendar/Application/Search/CalendarCommandPaletteProvider.cs`

### Frontend (~15 files)

- `shared/components/command-palette/command-palette.component.ts`
- `shared/services/command-palette.service.ts`
- `shared/services/search.service.ts`
- `shared/graphql/search.operations.ts`
- `shared/models/search.models.ts`
- `core/nlp/nlp-parser.service.ts`
- `core/nlp/models.ts`
- `core/nlp/rules/de.rules.ts`
- `core/nlp/rules/en.rules.ts`
- `core/nlp/rules/date-parser.ts`
- `core/nlp/rules/time-parser.ts`

### Tests (~5 files)

- `tests/FamilyHub.Search.Tests/FamilyHub.Search.Tests.csproj`
- `tests/FamilyHub.Search.Tests/UniversalSearchQueryHandlerTests.cs`
- `tests/FamilyHub.Search.Tests/SearchRegistryTests.cs`
- `tests/FamilyHub.Search.Tests/CommandPaletteRegistryTests.cs`
- `tests/FamilyHub.TestCommon/Fakes/FakeSearchProvider.cs`

## Files to Modify

- `src/FamilyHub.Api/Program.cs` — `RegisterModule<SearchModule>()`
- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootQuery.cs` — Add `Search()` method
- `src/FamilyHub.Api/Features/Family/FamilyModule.cs` — Register search/command providers
- `src/FamilyHub.Api/Features/Calendar/CalendarModule.cs` — Register search/command providers
- `src/FamilyHub.Api/FamilyHub.slnx` — Add `FamilyHub.Search.Tests` project
- `src/frontend/.../shared/layout/layout.component.ts` — Add `CommandPaletteComponent`
- `src/frontend/.../shared/layout/top-bar/top-bar.component.ts` — Add search button with Ctrl+K

## Implementation Tasks

### Task 1: Save Spec, Commit, and Update GitHub Issue #208

1. Write spec files to `agent-os/specs/2026-03-03-universal-search-command-palette/`
2. Update GitHub issue #208 with structured title, body, and labels
3. Git commit spec files

### Task 2: Common/Search/ Infrastructure

Create `ISearchProvider`, `ICommandPaletteProvider`, registries, initializer, and DTOs. Mirror `Common/Widgets/` pattern.

### Task 3: SearchModule + GraphQL API

Create `SearchModule`, `UniversalSearchQuery` + handler, `SearchQuery` namespace type, wire into `RootQuery` and `Program.cs`.

### Task 4: Family Search & Command Providers

`FamilySearchProvider` (member name search), `FamilyCommandPaletteProvider` (invite, settings). Add `SearchByNameAsync` to repository.

### Task 5: Calendar Search & Command Providers

`CalendarSearchProvider`, `CalendarCommandPaletteProvider`. Add `SearchAsync` to calendar repository.

### Task 6: Frontend Command Palette Component

Overlay UI (Ctrl+K), keyboard navigation, grouped results, `CommandPaletteService` (signals). Integrate into layout.

### Task 7: Frontend Search Service + GraphQL

Apollo `SearchService`, GraphQL operations, TypeScript models. 300ms debounced search.

### Task 8: NLP Pattern Matching Engine

`NlpParserService` with German/English rules, date/time parsers. Top suggestion in palette.

### Task 9: Backend Unit Tests

`FamilyHub.Search.Tests` project. Tests for registries, query handler. `FakeSearchProvider`.

### Task 10: Accessibility & Polish

Focus trap, ARIA attributes, transitions, empty/loading states.

## Verification

1. `dotnet build` — compiles with new module
2. `dotnet test` — all tests pass (existing + new)
3. GraphQL playground: `{ search { universal(input: { query: "test" }) { results { title } } } }`
4. Frontend: Ctrl+K opens palette, search returns grouped results
5. NLP: "Morgen Termin um 10 Uhr" shows smart suggestion
6. Keyboard: Arrow keys navigate, Enter executes, Escape closes
