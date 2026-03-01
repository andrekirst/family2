# Search & Command Palette — Architecture & Implementation Guide

> **Issue:** [#208 — Overall search and command feature](https://github.com/andrekirst/family2/issues/208)
> **Status:** Design Document
> **Last Updated:** 2026-03-01

---

## Table of Contents

1. [Vision & Use Cases](#1-vision--use-cases)
2. [Architecture Overview](#2-architecture-overview)
3. [Implementation Phases](#3-implementation-phases)
4. [Backend Architecture](#4-backend-architecture)
5. [Frontend Architecture](#5-frontend-architecture)
6. [Natural Language Processing](#6-natural-language-processing)
7. [GraphQL API Design](#7-graphql-api-design)
8. [Data Flow & Sequence Diagrams](#8-data-flow--sequence-diagrams)
9. [Testing Strategy](#9-testing-strategy)
10. [Performance Considerations](#10-performance-considerations)
11. [Future Enhancements](#11-future-enhancements)

---

## 1. Vision & Use Cases

The Search & Command Palette provides a unified, keyboard-driven interface for navigating, searching, and performing actions across all Family Hub modules. Think **Cmd+K / Ctrl+K** — a single entry point for everything.

### Primary Use Cases

| # | User Input (Example) | Behavior | Category |
|---|----------------------|----------|----------|
| 1 | `Morgen Termin um 10 Uhr` | Navigate to calendar, prefill event creation with tomorrow at 10:00 | Natural Language → Action |
| 2 | `create event` | Navigate to calendar with empty event creation form | Command |
| 3 | `Turnen` | Show results across calendar events, files, family members matching "Turnen" | Cross-Domain Search |
| 4 | `invite member` | Navigate to family settings → invite dialog | Command |
| 5 | `> settings` | Navigate to settings page | Navigation Command |
| 6 | `@Anna` | Show all content related to family member "Anna" | Entity Search |

### Command Syntax Conventions

```
[plain text]      → Cross-domain search (default)
> [page]          → Navigate to page
: [action]        → Execute action (create event, invite member, etc.)
@ [member]        → Search by family member
# [tag/category]  → Search by tag or category
```

---

## 2. Architecture Overview

### High-Level Architecture Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                        FRONTEND                               │
│                                                               │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │              Command Palette Component                   │  │
│  │  ┌──────────────────────────────────────────────────┐   │  │
│  │  │  Search Input  [Ctrl+K]                          │   │  │
│  │  │  ┌────────────────────────────────────────────┐  │   │  │
│  │  │  │ > Type to search or enter a command...     │  │   │  │
│  │  │  └────────────────────────────────────────────┘  │   │  │
│  │  │                                                  │   │  │
│  │  │  ┌────────────── Results Panel ───────────────┐  │   │  │
│  │  │  │  COMMANDS          SEARCH RESULTS           │  │   │  │
│  │  │  │  ─────────         ──────────────           │  │   │  │
│  │  │  │  : Create event    📅 Turnen (Di 15:00)    │  │   │  │
│  │  │  │  : Invite member   📁 Turnen-Anmeldung.pdf │  │   │  │
│  │  │  │  > Calendar        👤 Anna (Member)         │  │   │  │
│  │  │  │  > Settings                                 │  │   │  │
│  │  │  └────────────────────────────────────────────┘  │   │  │
│  │  └──────────────────────────────────────────────────┘   │  │
│  │                                                         │  │
│  │  CommandPaletteService  ←→  SearchService               │  │
│  │  CommandRegistry             Apollo GraphQL Client       │  │
│  └─────────────────────────────────────────────────────────┘  │
│                              │                                │
│                         GraphQL Queries                       │
└──────────────────────────────┼────────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                        BACKEND (.NET 9)                       │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐   │
│  │                    SearchModule                        │   │
│  │                                                       │   │
│  │  SearchQuery ──→ SearchQueryHandler                   │   │
│  │                       │                               │   │
│  │          ┌────────────┼────────────┐                  │   │
│  │          ▼            ▼            ▼                   │   │
│  │   ┌───────────┐ ┌──────────┐ ┌──────────┐           │   │
│  │   │  Family   │ │ Calendar │ │  Files   │ ...more   │   │
│  │   │ Provider  │ │ Provider │ │ Provider │           │   │
│  │   └───────────┘ └──────────┘ └──────────┘           │   │
│  │          │            │            │                   │   │
│  │          ▼            ▼            ▼                   │   │
│  │   ┌───────────────────────────────────────┐           │   │
│  │   │     ISearchProvider (per module)       │           │   │
│  │   │     - SearchAsync(term, context)       │           │   │
│  │   │     - GetSupportedCategories()         │           │   │
│  │   └───────────────────────────────────────┘           │   │
│  └───────────────────────────────────────────────────────┘   │
│                                                               │
│                      PostgreSQL (RLS)                         │
└──────────────────────────────────────────────────────────────┘
```

### Key Design Decisions

1. **Search Module as orchestrator** — A dedicated `SearchModule` aggregates results from per-module `ISearchProvider` implementations. This respects module boundaries per ADR-001.

2. **Provider pattern** — Each module registers its own `ISearchProvider`. The search module never directly accesses other modules' repositories.

3. **Client-side command registry** — Navigation commands and actions are registered on the frontend (no backend round-trip needed for `: create event` or `> settings`).

4. **Server-side cross-domain search** — Full-text search across data requires the backend, with permission filtering and RLS enforcement.

5. **Progressive NLP** — Phase 1 uses pattern matching for natural language; Phase 3 adds a proper NLP parser.

---

## 3. Implementation Phases

### Phase 1: Command Palette + Static Commands (Frontend Only)

**Goal:** Ship a working Ctrl+K palette with navigation and action commands.

```
Scope:
├── Command palette UI component (overlay, keyboard shortcuts)
├── Navigation commands (> dashboard, > calendar, > settings, etc.)
├── Action commands (: create event, : invite member, etc.)
├── Recent commands history (localStorage)
└── Keyboard navigation (↑↓, Enter, Escape)
```

**No backend changes needed.** All commands are statically registered in the frontend.

### Phase 2: Cross-Domain Search (Backend + Frontend)

**Goal:** Add server-side search across all modules the user has access to.

```
Scope:
├── Backend SearchModule with ISearchProvider interface
├── Search providers for Family, Calendar, FileManagement
├── GraphQL search query with unified result type
├── Frontend search results display (grouped by domain)
├── Debounced search-as-you-type
└── Permission-filtered results
```

### Phase 3: Natural Language Understanding

**Goal:** Parse natural language input into structured actions.

```
Scope:
├── NLP intent parser (pattern-based, bilingual DE/EN)
├── Date/time extraction ("morgen um 10", "next Tuesday at 3pm")
├── Entity extraction (member names, event titles)
├── Action mapping (intent → command + prefilled data)
└── Context panel prefill with parsed data
```

### Phase 4: Smart Suggestions & Learning

**Goal:** Personalized, context-aware suggestions.

```
Scope:
├── Frequently used commands (weighted by usage)
├── Time-based suggestions ("It's Monday → create weekly plan?")
├── Recent search history (server-side, per user)
└── Fuzzy matching and typo tolerance
```

---

## 4. Backend Architecture

### 4.1 Search Module Structure

Following the established feature-folder layout:

```
src/FamilyHub.Api/Features/Search/
├── SearchModule.cs                           # IModule registration
├── Domain/
│   ├── ValueObjects/
│   │   └── SearchTerm.cs                     # Vogen value object
│   └── Services/
│       └── ISearchProvider.cs                # Provider interface
├── Application/
│   ├── Queries/
│   │   ├── UnifiedSearch/
│   │   │   ├── UnifiedSearchQuery.cs         # Query record
│   │   │   ├── UnifiedSearchQueryHandler.cs  # Aggregating handler
│   │   │   ├── UnifiedSearchResult.cs        # Result record
│   │   │   └── QueryType.cs                  # GraphQL query type
│   │   └── SearchSuggestions/
│   │       ├── SearchSuggestionsQuery.cs
│   │       └── SearchSuggestionsQueryHandler.cs
│   └── Models/
│       ├── SearchResultItem.cs
│       └── SearchCategory.cs
└── Infrastructure/
    └── Providers/                            # Per-module search providers
        ├── FamilySearchProvider.cs
        ├── CalendarSearchProvider.cs
        └── FileSearchProvider.cs
```

### 4.2 ISearchProvider Interface

Each module contributes a search provider — this is the extension point:

```csharp
// src/FamilyHub.Api/Features/Search/Domain/Services/ISearchProvider.cs

namespace FamilyHub.Api.Features.Search.Domain.Services;

/// <summary>
/// Interface for module-specific search providers.
/// Each module implements this to contribute search results
/// to the unified search palette.
/// </summary>
public interface ISearchProvider
{
    /// <summary>
    /// Unique identifier for this provider (e.g., "calendar", "family", "files").
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Display name shown in search result grouping headers.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Icon identifier for the frontend (maps to icon set).
    /// </summary>
    string Icon { get; }

    /// <summary>
    /// Priority for result ordering (lower = higher priority).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Execute a search within this module's domain.
    /// </summary>
    /// <param name="term">The validated search term.</param>
    /// <param name="context">Search context (user, family, permissions).</param>
    /// <param name="maxResults">Maximum results to return from this provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of search result items from this module.</returns>
    Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        string term,
        SearchContext context,
        int maxResults,
        CancellationToken cancellationToken);
}

/// <summary>
/// Context passed to each search provider for authorization and scoping.
/// </summary>
public sealed record SearchContext(
    UserId UserId,
    FamilyId FamilyId,
    IReadOnlyList<string> Permissions
);
```

### 4.3 SearchTerm Value Object

Following the Vogen pattern used throughout the codebase:

```csharp
// src/FamilyHub.Api/Features/Search/Domain/ValueObjects/SearchTerm.cs

using Vogen;

namespace FamilyHub.Api.Features.Search.Domain.ValueObjects;

/// <summary>
/// Validated search term. Enforces minimum length and sanitization.
/// </summary>
[ValueObject<string>]
public readonly partial struct SearchTerm
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Search term cannot be empty");
        if (value.Trim().Length < 2)
            return Validation.Invalid("Search term must be at least 2 characters");
        if (value.Length > 200)
            return Validation.Invalid("Search term cannot exceed 200 characters");
        return Validation.Ok;
    }

    private static string NormalizeInput(string input) => input.Trim();
}
```

### 4.4 Search Result Model

```csharp
// src/FamilyHub.Api/Features/Search/Application/Models/SearchResultItem.cs

namespace FamilyHub.Api.Features.Search.Application.Models;

/// <summary>
/// A single search result item from any module.
/// </summary>
public sealed record SearchResultItem(
    /// <summary>Unique identifier of the matched entity.</summary>
    string Id,

    /// <summary>Display title (e.g., event name, file name, member name).</summary>
    string Title,

    /// <summary>Optional subtitle/description for context.</summary>
    string? Subtitle,

    /// <summary>Source category (e.g., "calendar", "files", "family").</summary>
    string Category,

    /// <summary>Icon identifier for the result type.</summary>
    string Icon,

    /// <summary>Frontend route to navigate to when selected.</summary>
    string Route,

    /// <summary>
    /// Optional route parameters for deep linking
    /// (e.g., { "date": "2026-03-02", "eventId": "abc-123" }).
    /// </summary>
    Dictionary<string, string>? RouteParams,

    /// <summary>Relevance score for ranking (0.0 – 1.0).</summary>
    double Score,

    /// <summary>Optional metadata for rich result display.</summary>
    Dictionary<string, string>? Metadata
);

/// <summary>
/// Grouped search results returned to the frontend.
/// </summary>
public sealed record UnifiedSearchResult(
    IReadOnlyList<SearchResultGroup> Groups,
    int TotalCount,
    TimeSpan SearchDuration
);

/// <summary>
/// A group of results from a single provider/category.
/// </summary>
public sealed record SearchResultGroup(
    string Category,
    string DisplayName,
    string Icon,
    IReadOnlyList<SearchResultItem> Items,
    int TotalInCategory
);
```

### 4.5 Unified Search Query & Handler

```csharp
// src/FamilyHub.Api/Features/Search/Application/Queries/UnifiedSearch/UnifiedSearchQuery.cs

using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Search.Application.Models;

namespace FamilyHub.Api.Features.Search.Application.Queries.UnifiedSearch;

public sealed record UnifiedSearchQuery(
    string Term,
    UserId UserId,
    FamilyId FamilyId,
    IReadOnlyList<string>? Categories,   // null = search all
    int MaxResultsPerCategory = 5
) : IQuery<UnifiedSearchResult>;
```

```csharp
// src/FamilyHub.Api/Features/Search/Application/Queries/UnifiedSearch/UnifiedSearchQueryHandler.cs

using System.Diagnostics;
using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Search.Application.Models;
using FamilyHub.Api.Features.Search.Domain.Services;

namespace FamilyHub.Api.Features.Search.Application.Queries.UnifiedSearch;

public sealed class UnifiedSearchQueryHandler(
    IEnumerable<ISearchProvider> providers)
    : IQueryHandler<UnifiedSearchQuery, UnifiedSearchResult>
{
    public async ValueTask<UnifiedSearchResult> Handle(
        UnifiedSearchQuery query,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // Build search context for authorization
        var context = new SearchContext(
            query.UserId,
            query.FamilyId,
            Permissions: [] // Populated from user's role in actual implementation
        );

        // Filter providers by requested categories (or use all)
        var activeProviders = query.Categories is { Count: > 0 }
            ? providers.Where(p => query.Categories.Contains(p.Category))
            : providers;

        // Execute all provider searches in parallel
        var searchTasks = activeProviders
            .Select(async provider =>
            {
                var items = await provider.SearchAsync(
                    query.Term,
                    context,
                    query.MaxResultsPerCategory,
                    cancellationToken);

                return new SearchResultGroup(
                    Category: provider.Category,
                    DisplayName: provider.DisplayName,
                    Icon: provider.Icon,
                    Items: items,
                    TotalInCategory: items.Count
                );
            });

        var groups = await Task.WhenAll(searchTasks);

        stopwatch.Stop();

        // Filter out empty groups and sort by provider priority
        var nonEmptyGroups = groups
            .Where(g => g.Items.Count > 0)
            .OrderBy(g => providers.First(p => p.Category == g.Category).Priority)
            .ToList();

        return new UnifiedSearchResult(
            Groups: nonEmptyGroups,
            TotalCount: nonEmptyGroups.Sum(g => g.TotalInCategory),
            SearchDuration: stopwatch.Elapsed
        );
    }
}
```

### 4.6 Example Search Provider — Calendar

```csharp
// src/FamilyHub.Api/Features/Search/Infrastructure/Providers/CalendarSearchProvider.cs

using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Search.Application.Models;
using FamilyHub.Api.Features.Search.Domain.Services;

namespace FamilyHub.Api.Features.Search.Infrastructure.Providers;

public sealed class CalendarSearchProvider(
    ICalendarEventRepository calendarEventRepository)
    : ISearchProvider
{
    public string Category => "calendar";
    public string DisplayName => "Calendar Events";
    public string Icon => "CALENDAR";
    public int Priority => 10;

    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        string term,
        SearchContext context,
        int maxResults,
        CancellationToken cancellationToken)
    {
        var events = await calendarEventRepository.SearchByTitleAsync(
            context.FamilyId,
            term,
            maxResults,
            cancellationToken);

        return events.Select(e => new SearchResultItem(
            Id: e.Id.Value.ToString(),
            Title: e.Title.Value,
            Subtitle: $"{e.StartTime:ddd, dd MMM yyyy HH:mm}",
            Category: Category,
            Icon: e.IsAllDay ? "CALENDAR_DAY" : "CALENDAR",
            Route: "/calendar",
            RouteParams: new Dictionary<string, string>
            {
                ["date"] = e.StartTime.ToString("yyyy-MM-dd"),
                ["eventId"] = e.Id.Value.ToString()
            },
            Score: CalculateRelevance(e.Title.Value, term),
            Metadata: new Dictionary<string, string>
            {
                ["startTime"] = e.StartTime.ToString("o"),
                ["endTime"] = e.EndTime.ToString("o"),
                ["isAllDay"] = e.IsAllDay.ToString()
            }
        )).ToList();
    }

    private static double CalculateRelevance(string title, string term)
    {
        if (title.Equals(term, StringComparison.OrdinalIgnoreCase))
            return 1.0;
        if (title.StartsWith(term, StringComparison.OrdinalIgnoreCase))
            return 0.9;
        if (title.Contains(term, StringComparison.OrdinalIgnoreCase))
            return 0.7;
        return 0.5;
    }
}
```

### 4.7 Example Search Provider — Family Members

```csharp
// src/FamilyHub.Api/Features/Search/Infrastructure/Providers/FamilySearchProvider.cs

using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Search.Application.Models;
using FamilyHub.Api.Features.Search.Domain.Services;

namespace FamilyHub.Api.Features.Search.Infrastructure.Providers;

public sealed class FamilySearchProvider(
    IFamilyMemberRepository familyMemberRepository)
    : ISearchProvider
{
    public string Category => "family";
    public string DisplayName => "Family Members";
    public string Icon => "USERS";
    public int Priority => 20;

    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        string term,
        SearchContext context,
        int maxResults,
        CancellationToken cancellationToken)
    {
        var members = await familyMemberRepository.SearchByNameAsync(
            context.FamilyId,
            term,
            maxResults,
            cancellationToken);

        return members.Select(m => new SearchResultItem(
            Id: m.Id.Value.ToString(),
            Title: m.UserName,
            Subtitle: $"Role: {m.Role.Value}",
            Category: Category,
            Icon: "USER",
            Route: "/family/settings",
            RouteParams: new Dictionary<string, string>
            {
                ["memberId"] = m.Id.Value.ToString()
            },
            Score: CalculateRelevance(m.UserName, term),
            Metadata: new Dictionary<string, string>
            {
                ["email"] = m.UserEmail,
                ["role"] = m.Role.Value,
                ["avatarId"] = m.AvatarId?.ToString() ?? ""
            }
        )).ToList();
    }

    private static double CalculateRelevance(string name, string term)
    {
        if (name.Equals(term, StringComparison.OrdinalIgnoreCase))
            return 1.0;
        if (name.StartsWith(term, StringComparison.OrdinalIgnoreCase))
            return 0.9;
        return 0.6;
    }
}
```

### 4.8 SearchModule Registration

```csharp
// src/FamilyHub.Api/Features/Search/SearchModule.cs

using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Features.Search.Domain.Services;
using FamilyHub.Api.Features.Search.Infrastructure.Providers;

namespace FamilyHub.Api.Features.Search;

public sealed class SearchModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Register all search providers
        // Each module's provider is registered here to keep search orchestration centralized
        services.AddScoped<ISearchProvider, CalendarSearchProvider>();
        services.AddScoped<ISearchProvider, FamilySearchProvider>();
        services.AddScoped<ISearchProvider, FileSearchProvider>();
        // Future: services.AddScoped<ISearchProvider, TaskSearchProvider>();
        // Future: services.AddScoped<ISearchProvider, ShoppingSearchProvider>();
    }
}
```

Registration in `Program.cs`:

```csharp
// Add after existing module registrations:
builder.Services.RegisterModule<SearchModule>(builder.Configuration);
```

---

## 5. Frontend Architecture

### 5.1 Feature Structure

```
src/frontend/family-hub-web/src/app/features/search/
├── components/
│   ├── command-palette/
│   │   ├── command-palette.component.ts       # Main overlay component
│   │   ├── command-palette.component.html     # Template
│   │   └── command-palette.component.css      # Styles
│   ├── search-result-item/
│   │   ├── search-result-item.component.ts    # Individual result row
│   │   └── search-result-item.component.html
│   └── search-result-group/
│       ├── search-result-group.component.ts   # Grouped results header + items
│       └── search-result-group.component.html
├── services/
│   ├── command-palette.service.ts             # Open/close state, keyboard handling
│   ├── command-registry.service.ts            # Static command definitions
│   └── search.service.ts                      # GraphQL search integration
├── models/
│   ├── command.model.ts                       # Command types
│   └── search-result.model.ts                 # Search result types
├── graphql/
│   └── search.operations.ts                   # GraphQL queries
├── search.providers.ts                        # Feature provider function
└── search.routes.ts                           # (optional, if full-page search needed)
```

### 5.2 Command & Search Result Models

```typescript
// src/app/features/search/models/command.model.ts

export type CommandCategory = 'navigation' | 'action' | 'search';

export interface Command {
  /** Unique identifier */
  id: string;
  /** Display label (e.g., "Create Event") */
  label: string;
  /** Optional description shown below label */
  description?: string;
  /** Category for grouping */
  category: CommandCategory;
  /** Icon identifier from icon set */
  icon: string;
  /** Keyboard shortcut hint (e.g., "Ctrl+Shift+E") */
  shortcut?: string;
  /** Search keywords that match this command */
  keywords: string[];
  /** Action to execute when selected */
  action: () => void;
  /** Required permission to show this command (null = always visible) */
  permission?: string;
}
```

```typescript
// src/app/features/search/models/search-result.model.ts

export interface SearchResultItem {
  id: string;
  title: string;
  subtitle?: string;
  category: string;
  icon: string;
  route: string;
  routeParams?: Record<string, string>;
  score: number;
  metadata?: Record<string, string>;
}

export interface SearchResultGroup {
  category: string;
  displayName: string;
  icon: string;
  items: SearchResultItem[];
  totalInCategory: number;
}

export interface UnifiedSearchResult {
  groups: SearchResultGroup[];
  totalCount: number;
  searchDuration: string;
}
```

### 5.3 Command Registry Service

```typescript
// src/app/features/search/services/command-registry.service.ts

import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Command } from '../models/command.model';
import { FamilyPermissionService } from '../../../core/permissions/family-permission.service';
import { ContextPanelService } from '../../../shared/services/context-panel.service';

@Injectable({ providedIn: 'root' })
export class CommandRegistryService {
  private readonly router = inject(Router);
  private readonly permissions = inject(FamilyPermissionService);
  private readonly contextPanel = inject(ContextPanelService);

  /**
   * All registered commands. Each feature module can extend this
   * by injecting additional commands via the registry.
   */
  getCommands(): Command[] {
    return [
      // --- Navigation Commands ---
      {
        id: 'nav-dashboard',
        label: $localize`:@@search.cmd.dashboard:Dashboard`,
        category: 'navigation',
        icon: 'HOME',
        keywords: ['dashboard', 'home', 'start', 'übersicht'],
        action: () => this.router.navigate(['/dashboard']),
      },
      {
        id: 'nav-calendar',
        label: $localize`:@@search.cmd.calendar:Calendar`,
        category: 'navigation',
        icon: 'CALENDAR',
        keywords: ['calendar', 'kalender', 'termine', 'events'],
        action: () => this.router.navigate(['/calendar']),
      },
      {
        id: 'nav-files',
        label: $localize`:@@search.cmd.files:Files`,
        category: 'navigation',
        icon: 'FOLDER',
        keywords: ['files', 'dateien', 'documents', 'dokumente'],
        action: () => this.router.navigate(['/files']),
      },
      {
        id: 'nav-family-settings',
        label: $localize`:@@search.cmd.familySettings:Family Settings`,
        category: 'navigation',
        icon: 'USERS',
        keywords: ['family', 'settings', 'familie', 'einstellungen', 'members', 'mitglieder'],
        action: () => this.router.navigate(['/family/settings']),
      },
      {
        id: 'nav-settings',
        label: $localize`:@@search.cmd.settings:Settings`,
        category: 'navigation',
        icon: 'SETTINGS',
        keywords: ['settings', 'einstellungen', 'preferences', 'config'],
        action: () => this.router.navigate(['/settings']),
      },
      {
        id: 'nav-event-chains',
        label: $localize`:@@search.cmd.eventChains:Automations`,
        category: 'navigation',
        icon: 'BOLT',
        keywords: ['automations', 'event chains', 'automatisierung', 'workflows'],
        action: () => this.router.navigate(['/event-chains']),
      },

      // --- Action Commands ---
      {
        id: 'action-create-event',
        label: $localize`:@@search.cmd.createEvent:Create Event`,
        description: $localize`:@@search.cmd.createEvent.desc:Create a new calendar event`,
        category: 'action',
        icon: 'CALENDAR_PLUS',
        shortcut: 'Ctrl+Shift+E',
        keywords: ['create event', 'new event', 'termin erstellen', 'neuer termin', 'add event'],
        action: () => {
          this.router.navigate(['/calendar']);
          // Open context panel in create mode after navigation
          setTimeout(() => this.contextPanel.openCreate(), 100);
        },
      },
      {
        id: 'action-invite-member',
        label: $localize`:@@search.cmd.inviteMember:Invite Family Member`,
        description: $localize`:@@search.cmd.inviteMember.desc:Send an invitation to join your family`,
        category: 'action',
        icon: 'USER_PLUS',
        keywords: ['invite', 'member', 'einladen', 'mitglied', 'add member'],
        permission: 'family:invite',
        action: () => this.router.navigate(['/family/settings'], {
          queryParams: { action: 'invite' },
        }),
      },
      {
        id: 'action-upload-file',
        label: $localize`:@@search.cmd.uploadFile:Upload File`,
        description: $localize`:@@search.cmd.uploadFile.desc:Upload a file to your family storage`,
        category: 'action',
        icon: 'UPLOAD',
        keywords: ['upload', 'file', 'hochladen', 'datei'],
        action: () => this.router.navigate(['/files'], {
          queryParams: { action: 'upload' },
        }),
      },
    ];
  }

  /**
   * Filter commands by search term and user permissions.
   */
  filterCommands(term: string): Command[] {
    const normalizedTerm = term.toLowerCase().trim();

    return this.getCommands()
      .filter(cmd => {
        // Check permission
        if (cmd.permission && !this.permissions.hasPermission(cmd.permission)) {
          return false;
        }

        // Match against label, description, and keywords
        const searchableText = [
          cmd.label.toLowerCase(),
          cmd.description?.toLowerCase() ?? '',
          ...cmd.keywords.map(k => k.toLowerCase()),
        ].join(' ');

        return searchableText.includes(normalizedTerm);
      });
  }
}
```

### 5.4 Command Palette Service

```typescript
// src/app/features/search/services/command-palette.service.ts

import { Injectable, signal, computed, inject, OnDestroy } from '@angular/core';
import { CommandRegistryService } from './command-registry.service';
import { SearchService } from './search.service';
import { Command } from '../models/command.model';
import { SearchResultGroup } from '../models/search-result.model';

export type PaletteMode = 'closed' | 'commands' | 'search' | 'mixed';

@Injectable({ providedIn: 'root' })
export class CommandPaletteService implements OnDestroy {
  private readonly commandRegistry = inject(CommandRegistryService);
  private readonly searchService = inject(SearchService);

  // --- State Signals ---
  readonly isOpen = signal(false);
  readonly inputValue = signal('');
  readonly selectedIndex = signal(0);
  readonly isSearching = signal(false);

  // --- Computed State ---
  readonly mode = computed<PaletteMode>(() => {
    if (!this.isOpen()) return 'closed';
    const value = this.inputValue();
    if (!value) return 'commands'; // Show recent/frequent commands
    if (value.startsWith('>')) return 'commands'; // Navigation mode
    if (value.startsWith(':')) return 'commands'; // Action mode
    return 'mixed'; // Show both commands and search results
  });

  readonly matchingCommands = computed(() => {
    const value = this.inputValue();
    if (!value) {
      return this.commandRegistry.getCommands().slice(0, 6); // Show top commands
    }

    // Strip prefix for filtering
    const term = value.replace(/^[>:]/, '').trim();
    if (!term) return this.commandRegistry.getCommands();

    return this.commandRegistry.filterCommands(term);
  });

  readonly searchResults = signal<SearchResultGroup[]>([]);

  readonly allItems = computed(() => {
    const commands = this.matchingCommands();
    const results = this.searchResults();
    return {
      commands,
      searchGroups: results,
      totalCount: commands.length + results.reduce((sum, g) => sum + g.items.length, 0),
    };
  });

  // --- Keyboard Listener ---
  private keydownHandler = (event: KeyboardEvent) => {
    // Ctrl+K or Cmd+K to toggle
    if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
      event.preventDefault();
      this.toggle();
    }

    // Escape to close
    if (event.key === 'Escape' && this.isOpen()) {
      this.close();
    }
  };

  constructor() {
    document.addEventListener('keydown', this.keydownHandler);
  }

  ngOnDestroy(): void {
    document.removeEventListener('keydown', this.keydownHandler);
  }

  // --- Public API ---

  toggle(): void {
    if (this.isOpen()) {
      this.close();
    } else {
      this.open();
    }
  }

  open(): void {
    this.isOpen.set(true);
    this.inputValue.set('');
    this.selectedIndex.set(0);
    this.searchResults.set([]);
  }

  close(): void {
    this.isOpen.set(false);
    this.inputValue.set('');
    this.selectedIndex.set(0);
  }

  onInputChange(value: string): void {
    this.inputValue.set(value);
    this.selectedIndex.set(0);

    // Trigger server-side search for non-command inputs
    if (value.length >= 2 && !value.startsWith('>') && !value.startsWith(':')) {
      this.searchService.search(value);
    } else {
      this.searchResults.set([]);
    }
  }

  moveSelection(direction: 'up' | 'down'): void {
    const total = this.allItems().totalCount;
    if (total === 0) return;

    const current = this.selectedIndex();
    if (direction === 'down') {
      this.selectedIndex.set((current + 1) % total);
    } else {
      this.selectedIndex.set((current - 1 + total) % total);
    }
  }

  executeSelected(): void {
    const items = this.allItems();
    const index = this.selectedIndex();

    // Check if selection is a command
    if (index < items.commands.length) {
      const command = items.commands[index];
      command.action();
      this.close();
      return;
    }

    // Otherwise it's a search result — navigate to it
    const searchIndex = index - items.commands.length;
    let currentIndex = 0;
    for (const group of items.searchGroups) {
      for (const item of group.items) {
        if (currentIndex === searchIndex) {
          this.navigateToResult(item);
          this.close();
          return;
        }
        currentIndex++;
      }
    }
  }

  private navigateToResult(item: { route: string; routeParams?: Record<string, string> }): void {
    // Navigation is handled by the component via Router
    // This service just manages state
  }
}
```

### 5.5 Search Service (GraphQL Integration)

```typescript
// src/app/features/search/services/search.service.ts

import { Injectable, inject, signal } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Subject, debounceTime, distinctUntilChanged, switchMap, catchError, of } from 'rxjs';
import { UNIFIED_SEARCH_QUERY } from '../graphql/search.operations';
import { SearchResultGroup, UnifiedSearchResult } from '../models/search-result.model';

@Injectable({ providedIn: 'root' })
export class SearchService {
  private readonly apollo = inject(Apollo);

  readonly isLoading = signal(false);
  readonly results = signal<SearchResultGroup[]>([]);
  readonly error = signal<string | null>(null);

  private searchSubject = new Subject<string>();

  constructor() {
    this.searchSubject
      .pipe(
        debounceTime(300), // Wait 300ms after last keystroke
        distinctUntilChanged(),
        switchMap(term => {
          if (term.length < 2) {
            return of(null);
          }

          this.isLoading.set(true);
          this.error.set(null);

          return this.apollo
            .query<{ search: { unified: UnifiedSearchResult } }>({
              query: UNIFIED_SEARCH_QUERY,
              variables: { term, maxResultsPerCategory: 5 },
              fetchPolicy: 'network-only',
            })
            .pipe(
              catchError(err => {
                console.error('Search failed:', err);
                this.error.set('Search failed');
                return of(null);
              }),
            );
        }),
      )
      .subscribe(result => {
        this.isLoading.set(false);
        if (result?.data?.search?.unified) {
          this.results.set(result.data.search.unified.groups);
        } else {
          this.results.set([]);
        }
      });
  }

  search(term: string): void {
    this.searchSubject.next(term);
  }
}
```

### 5.6 GraphQL Operations

```typescript
// src/app/features/search/graphql/search.operations.ts

import { gql } from 'apollo-angular';

export const UNIFIED_SEARCH_QUERY = gql`
  query UnifiedSearch($term: String!, $categories: [String!], $maxResultsPerCategory: Int) {
    search {
      unified(
        input: {
          term: $term
          categories: $categories
          maxResultsPerCategory: $maxResultsPerCategory
        }
      ) {
        groups {
          category
          displayName
          icon
          totalInCategory
          items {
            id
            title
            subtitle
            category
            icon
            route
            routeParams
            score
            metadata
          }
        }
        totalCount
        searchDuration
      }
    }
  }
`;
```

### 5.7 Command Palette Component

```typescript
// src/app/features/search/components/command-palette/command-palette.component.ts

import {
  Component,
  inject,
  signal,
  effect,
  ViewChild,
  ElementRef,
  AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommandPaletteService } from '../../services/command-palette.service';
import { SearchService } from '../../services/search.service';
import { SearchResultGroupComponent } from '../search-result-group/search-result-group.component';

@Component({
  selector: 'app-command-palette',
  standalone: true,
  imports: [CommonModule, FormsModule, SearchResultGroupComponent],
  template: `
    @if (paletteService.isOpen()) {
      <!-- Backdrop -->
      <div
        class="fixed inset-0 bg-black/50 z-50 flex items-start justify-center pt-[15vh]"
        (click)="paletteService.close()"
        (keydown.escape)="paletteService.close()"
      >
        <!-- Palette Container -->
        <div
          class="bg-white rounded-xl shadow-2xl w-full max-w-2xl max-h-[60vh] flex flex-col overflow-hidden"
          (click)="$event.stopPropagation()"
        >
          <!-- Search Input -->
          <div class="flex items-center px-4 border-b border-gray-200">
            <svg class="w-5 h-5 text-gray-400 mr-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input
              #searchInput
              type="text"
              [value]="paletteService.inputValue()"
              (input)="paletteService.onInputChange($any($event.target).value)"
              (keydown.arrowDown)="paletteService.moveSelection('down'); $event.preventDefault()"
              (keydown.arrowUp)="paletteService.moveSelection('up'); $event.preventDefault()"
              (keydown.enter)="paletteService.executeSelected(); $event.preventDefault()"
              class="flex-1 py-4 text-lg text-gray-900 placeholder-gray-400 bg-transparent border-none outline-none"
              [placeholder]="placeholderText"
              autocomplete="off"
              data-testid="command-palette-input"
            />
            <kbd class="hidden sm:inline-flex items-center px-2 py-1 text-xs text-gray-400 bg-gray-100 rounded">
              ESC
            </kbd>
          </div>

          <!-- Results -->
          <div class="overflow-y-auto flex-1" data-testid="command-palette-results">
            <!-- Commands Section -->
            @if (paletteService.matchingCommands().length > 0) {
              <div class="px-2 py-2">
                <div class="px-3 py-1.5 text-xs font-semibold text-gray-500 uppercase tracking-wider"
                     i18n="@@search.commands">
                  Commands
                </div>
                @for (cmd of paletteService.matchingCommands(); track cmd.id; let i = $index) {
                  <button
                    class="w-full flex items-center px-3 py-2 rounded-lg text-left transition-colors"
                    [class.bg-blue-50]="paletteService.selectedIndex() === i"
                    [class.text-blue-700]="paletteService.selectedIndex() === i"
                    [class.hover:bg-gray-50]="paletteService.selectedIndex() !== i"
                    (click)="cmd.action(); paletteService.close()"
                    (mouseenter)="paletteService.selectedIndex.set(i)"
                    [attr.data-testid]="'cmd-' + cmd.id"
                  >
                    <span class="w-8 h-8 flex items-center justify-center rounded-md bg-gray-100 mr-3">
                      <!-- Icon placeholder — use your ICONS system here -->
                      <span class="text-sm">{{ cmd.icon === 'CALENDAR' ? '📅' :
                        cmd.icon === 'HOME' ? '🏠' :
                        cmd.icon === 'USERS' ? '👥' :
                        cmd.icon === 'FOLDER' ? '📁' :
                        cmd.icon === 'SETTINGS' ? '⚙️' :
                        cmd.icon === 'BOLT' ? '⚡' : '🔍' }}</span>
                    </span>
                    <div class="flex-1 min-w-0">
                      <div class="text-sm font-medium truncate">{{ cmd.label }}</div>
                      @if (cmd.description) {
                        <div class="text-xs text-gray-500 truncate">{{ cmd.description }}</div>
                      }
                    </div>
                    @if (cmd.shortcut) {
                      <kbd class="ml-2 px-2 py-0.5 text-xs text-gray-400 bg-gray-100 rounded">
                        {{ cmd.shortcut }}
                      </kbd>
                    }
                  </button>
                }
              </div>
            }

            <!-- Search Results Section -->
            @if (searchService.isLoading()) {
              <div class="px-4 py-8 text-center text-gray-400">
                <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-500 mx-auto mb-2"></div>
                <span class="text-sm" i18n="@@search.searching">Searching...</span>
              </div>
            }

            @for (group of searchService.results(); track group.category) {
              <app-search-result-group
                [group]="group"
                [selectedIndex]="paletteService.selectedIndex()"
                [indexOffset]="getGroupOffset(group)"
                (resultSelected)="onResultSelected($event)"
                (resultHovered)="paletteService.selectedIndex.set($event)"
              />
            }

            <!-- Empty State -->
            @if (paletteService.inputValue().length >= 2 &&
                 !searchService.isLoading() &&
                 paletteService.matchingCommands().length === 0 &&
                 searchService.results().length === 0) {
              <div class="px-4 py-8 text-center text-gray-400">
                <span class="text-sm" i18n="@@search.noResults">No results found</span>
              </div>
            }
          </div>

          <!-- Footer -->
          <div class="px-4 py-2 border-t border-gray-100 flex items-center justify-between text-xs text-gray-400">
            <div class="flex items-center gap-3">
              <span><kbd class="px-1.5 py-0.5 bg-gray-100 rounded">↑↓</kbd>
                <span i18n="@@search.navigate"> navigate</span></span>
              <span><kbd class="px-1.5 py-0.5 bg-gray-100 rounded">↵</kbd>
                <span i18n="@@search.select"> select</span></span>
              <span><kbd class="px-1.5 py-0.5 bg-gray-100 rounded">esc</kbd>
                <span i18n="@@search.close"> close</span></span>
            </div>
            <div class="flex items-center gap-2">
              <span><kbd class="px-1.5 py-0.5 bg-gray-100 rounded">></kbd>
                <span i18n="@@search.pages"> pages</span></span>
              <span><kbd class="px-1.5 py-0.5 bg-gray-100 rounded">:</kbd>
                <span i18n="@@search.actions"> actions</span></span>
            </div>
          </div>
        </div>
      </div>
    }
  `,
})
export class CommandPaletteComponent implements AfterViewInit {
  readonly paletteService = inject(CommandPaletteService);
  readonly searchService = inject(SearchService);
  private readonly router = inject(Router);

  @ViewChild('searchInput') searchInput?: ElementRef<HTMLInputElement>;

  placeholderText = $localize`:@@search.placeholder:Search or type a command...`;

  private focusEffect = effect(() => {
    if (this.paletteService.isOpen()) {
      // Focus input when palette opens
      setTimeout(() => this.searchInput?.nativeElement?.focus(), 50);
    }
  });

  ngAfterViewInit(): void {
    // Sync search results back to palette service
    effect(() => {
      const results = this.searchService.results();
      this.paletteService.searchResults.set(results);
    });
  }

  getGroupOffset(group: { category: string }): number {
    const commands = this.paletteService.matchingCommands();
    let offset = commands.length;
    for (const g of this.searchService.results()) {
      if (g.category === group.category) break;
      offset += g.items.length;
    }
    return offset;
  }

  onResultSelected(result: { route: string; routeParams?: Record<string, string> }): void {
    this.paletteService.close();
    if (result.routeParams) {
      this.router.navigate([result.route], { queryParams: result.routeParams });
    } else {
      this.router.navigate([result.route]);
    }
  }
}
```

### 5.8 Integration into Layout

The command palette is added to the `LayoutComponent` so it's available on all protected pages:

```typescript
// Updated layout.component.ts imports and template

import { CommandPaletteComponent } from '../../features/search/components/command-palette/command-palette.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    SidebarComponent,
    TopBarComponent,
    ContextPanelComponent,
    ToastContainerComponent,
    CommandPaletteComponent,     // <-- Add this
  ],
  template: `
    <div class="min-h-screen bg-gray-50 flex">
      <app-sidebar />
      <div
        class="flex-1 flex flex-col transition-all duration-300 min-w-0"
        [style.margin-left]="sidebarState.isCollapsed() ? '64px' : '240px'"
      >
        <app-top-bar />
        <div class="flex-1 flex overflow-hidden">
          <main class="flex-1 overflow-hidden flex flex-col">
            <div class="flex-1 min-h-0 flex flex-col w-full">
              <router-outlet />
            </div>
          </main>
          <app-context-panel [isDesktop]="isDesktop()" />
        </div>
      </div>
    </div>
    <app-toast-container />
    <app-command-palette />     <!-- Add this -->
  `,
})
```

### 5.9 Search Trigger in Top Bar

Add a search button to the `TopBarComponent` for discoverability:

```html
<!-- Add to top-bar.component.ts template, before the actions div -->
<button
  (click)="openCommandPalette()"
  class="flex items-center gap-2 px-3 py-1.5 text-sm text-gray-500 bg-gray-100 rounded-lg
         hover:bg-gray-200 transition-colors"
  data-testid="search-trigger"
>
  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
          d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
  </svg>
  <span class="hidden md:inline" i18n="@@search.triggerLabel">Search...</span>
  <kbd class="hidden md:inline px-1.5 py-0.5 text-xs bg-white rounded border border-gray-300">
    Ctrl+K
  </kbd>
</button>
```

### 5.10 Feature Provider

```typescript
// src/app/features/search/search.providers.ts

import { Provider } from '@angular/core';
import { CommandPaletteService } from './services/command-palette.service';
import { CommandRegistryService } from './services/command-registry.service';
import { SearchService } from './services/search.service';

export function provideSearchFeature(): Provider[] {
  return [
    CommandPaletteService,
    CommandRegistryService,
    SearchService,
  ];
}
```

---

## 6. Natural Language Processing

### 6.1 NLP Strategy (Phase 3)

The NLP layer sits between the user's raw input and the command/search dispatch:

```
User Input
    │
    ▼
┌──────────────────┐
│  Intent Detector  │  ← Pattern matching (Phase 3a)
│                   │  ← ML model (Phase 3b, future)
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Entity Extractor │  ← Date/time parsing
│                   │  ← Member name matching
│                   │  ← Category detection
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Action Mapper    │  ← Maps intent + entities → Command
│                   │  ← Generates route + prefilled params
└──────────────────┘
```

### 6.2 Intent Detection (Pattern-Based)

```typescript
// src/app/features/search/services/nlp/intent-detector.service.ts

export type Intent =
  | 'create_event'
  | 'search'
  | 'navigate'
  | 'invite_member'
  | 'upload_file';

export interface DetectedIntent {
  intent: Intent;
  confidence: number;
  entities: Record<string, string>;
  originalInput: string;
}

const INTENT_PATTERNS: { intent: Intent; patterns: RegExp[] }[] = [
  {
    intent: 'create_event',
    patterns: [
      // German
      /(?:termin|event|veranstaltung)\s+(?:um|ab|von|erstellen|anlegen|neu)/i,
      /(?:morgen|heute|übermorgen|montag|dienstag|mittwoch|donnerstag|freitag|samstag|sonntag)\s+(?:termin|um)/i,
      // English
      /(?:create|new|add|schedule)\s+(?:event|appointment|meeting)/i,
      /(?:tomorrow|today|monday|tuesday|wednesday|thursday|friday|saturday|sunday)\s+(?:at|event)/i,
    ],
  },
  {
    intent: 'invite_member',
    patterns: [
      /(?:invite|einladen)\s+(?:member|mitglied|person)/i,
      /(?:add|hinzufügen)\s+(?:member|family|mitglied|familie)/i,
    ],
  },
];
```

### 6.3 Date/Time Extraction

```typescript
// src/app/features/search/services/nlp/datetime-extractor.service.ts

export interface ExtractedDateTime {
  date: Date;
  hasTime: boolean;
  hasDate: boolean;
  original: string;
}

const RELATIVE_DATE_PATTERNS: Record<string, () => Date> = {
  // German
  'heute': () => new Date(),
  'morgen': () => addDays(new Date(), 1),
  'übermorgen': () => addDays(new Date(), 2),
  // English
  'today': () => new Date(),
  'tomorrow': () => addDays(new Date(), 1),
};

const TIME_PATTERN = /(?:um|at|ab)\s+(\d{1,2})(?::(\d{2}))?\s*(?:uhr|Uhr|h)?/i;

// Example: "Morgen Termin um 10 Uhr"
//   → { date: 2026-03-02T10:00:00, hasTime: true, hasDate: true }
```

---

## 7. GraphQL API Design

### 7.1 Schema Extension

Following the namespace type pattern established in the codebase:

```graphql
# New namespace type for search queries
type SearchQuery {
  """
  Unified search across all modules the user has access to.
  """
  unified(input: UnifiedSearchInput!): UnifiedSearchResult!

  """
  Get search suggestions based on recent searches and popular queries.
  """
  suggestions(term: String!, limit: Int = 5): [SearchSuggestion!]!
}

# Add to RootQuery
type RootQuery {
  # ... existing fields ...

  """Search queries (unified cross-module search)."""
  search: SearchQuery!
}

# Input types
input UnifiedSearchInput {
  """The search term (minimum 2 characters)."""
  term: String!

  """Optional categories to filter (e.g., ["calendar", "files"]). Null = search all."""
  categories: [String!]

  """Maximum results per category (default: 5)."""
  maxResultsPerCategory: Int = 5
}

# Result types
type UnifiedSearchResult {
  groups: [SearchResultGroup!]!
  totalCount: Int!
  searchDuration: String!
}

type SearchResultGroup {
  category: String!
  displayName: String!
  icon: String!
  items: [SearchResultItem!]!
  totalInCategory: Int!
}

type SearchResultItem {
  id: String!
  title: String!
  subtitle: String
  category: String!
  icon: String!
  route: String!
  routeParams: JSON
  score: Float!
  metadata: JSON
}

type SearchSuggestion {
  text: String!
  category: String
  icon: String
}
```

### 7.2 GraphQL Namespace Type

```csharp
// src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/SearchQuery.cs

namespace FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;

/// <summary>
/// Namespace type for search-related queries.
/// </summary>
public class SearchQuery { }
```

Add to `RootQuery.cs`:

```csharp
/// <summary>
/// Search queries (unified cross-module search).
/// </summary>
[Authorize]
public SearchQuery Search() => new();
```

### 7.3 Query Type (GraphQL Resolver)

```csharp
// src/FamilyHub.Api/Features/Search/Application/Queries/UnifiedSearch/QueryType.cs

using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Search.Application.Models;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Search.Application.Queries.UnifiedSearch;

[ExtendObjectType(typeof(SearchQuery))]
public class QueryType
{
    [Authorize]
    public async Task<UnifiedSearchResult> Unified(
        UnifiedSearchInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserId = ExternalUserId.From(
            claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException());

        var user = await userRepository.GetByExternalIdAsync(externalUserId, cancellationToken)
                   ?? throw new UnauthorizedAccessException("User not found");

        var familyId = user.FamilyId
                       ?? throw new InvalidOperationException("User must belong to a family to search");

        var query = new UnifiedSearchQuery(
            Term: input.Term,
            UserId: user.Id,
            FamilyId: familyId,
            Categories: input.Categories,
            MaxResultsPerCategory: input.MaxResultsPerCategory ?? 5
        );

        return await queryBus.SendAsync(query, cancellationToken);
    }
}

/// <summary>
/// GraphQL input for unified search.
/// </summary>
public sealed record UnifiedSearchInput
{
    public required string Term { get; init; }
    public IReadOnlyList<string>? Categories { get; init; }
    public int? MaxResultsPerCategory { get; init; }
}
```

---

## 8. Data Flow & Sequence Diagrams

### 8.1 Command Execution Flow

```
User                 Frontend                    Router
 │                      │                          │
 │  Ctrl+K              │                          │
 │─────────────────────>│                          │
 │                      │ open palette             │
 │                      │─────────┐                │
 │                      │         │                │
 │  Types ": create     │<────────┘                │
 │  event"              │                          │
 │─────────────────────>│                          │
 │                      │ filterCommands()         │
 │                      │─────────┐                │
 │                      │         │ match:         │
 │  Shows matched       │<────────┘ "Create Event" │
 │  commands            │                          │
 │                      │                          │
 │  Press Enter         │                          │
 │─────────────────────>│                          │
 │                      │ command.action()         │
 │                      │────────────────────────>│
 │                      │                          │ navigate('/calendar')
 │                      │ close palette            │
 │                      │─────────┐                │
 │  Calendar page with  │<────────┘                │
 │  create panel open   │                          │
```

### 8.2 Cross-Domain Search Flow

```
User                Frontend              Apollo            Backend (GraphQL)         SearchModule
 │                     │                    │                      │                       │
 │  Types "Turnen"     │                    │                      │                       │
 │────────────────────>│                    │                      │                       │
 │                     │ debounce(300ms)    │                      │                       │
 │                     │──────┐             │                      │                       │
 │                     │      │             │                      │                       │
 │                     │<─────┘             │                      │                       │
 │                     │                    │                      │                       │
 │                     │ query UnifiedSearch│                      │                       │
 │                     │───────────────────>│                      │                       │
 │                     │                    │  POST /graphql       │                       │
 │                     │                    │─────────────────────>│                       │
 │                     │                    │                      │                       │
 │                     │                    │                      │ UnifiedSearchQuery     │
 │                     │                    │                      │──────────────────────>│
 │                     │                    │                      │                       │
 │                     │                    │                      │    ┌──── parallel ────┐│
 │                     │                    │                      │    │ CalendarProvider  ││
 │                     │                    │                      │    │ FamilyProvider    ││
 │                     │                    │                      │    │ FileProvider      ││
 │                     │                    │                      │    └──────────────────┘│
 │                     │                    │                      │                       │
 │                     │                    │                      │  UnifiedSearchResult   │
 │                     │                    │                      │<──────────────────────│
 │                     │                    │  GraphQL response    │                       │
 │                     │                    │<─────────────────────│                       │
 │                     │ SearchResultGroup[]│                      │                       │
 │                     │<──────────────────│                      │                       │
 │                     │                    │                      │                       │
 │  Results displayed: │                    │                      │                       │
 │  📅 Turnen (Di 15h) │                    │                      │                       │
 │  📁 Turnen-Anm.pdf  │                    │                      │                       │
 │<────────────────────│                    │                      │                       │
```

### 8.3 Natural Language Flow (Phase 3)

```
User                   Frontend                     NLP Layer              Calendar
 │                        │                             │                     │
 │ "Morgen Termin         │                             │                     │
 │  um 10 Uhr"           │                             │                     │
 │───────────────────────>│                             │                     │
 │                        │ detectIntent(input)         │                     │
 │                        │────────────────────────────>│                     │
 │                        │                             │                     │
 │                        │ DetectedIntent {            │                     │
 │                        │   intent: 'create_event',   │                     │
 │                        │   entities: {               │                     │
 │                        │     date: '2026-03-02',     │                     │
 │                        │     time: '10:00'           │                     │
 │                        │   }                         │                     │
 │                        │ }                           │                     │
 │                        │<────────────────────────────│                     │
 │                        │                             │                     │
 │                        │ navigate('/calendar', {     │                     │
 │                        │   date: '2026-03-02',       │                     │
 │                        │   createAt: '10:00'         │                     │
 │                        │ })                          │                     │
 │                        │──────────────────────────────────────────────────>│
 │                        │                             │                     │
 │  Calendar shows        │                             │                     │
 │  March 2 with event    │                             │  Open context panel │
 │  creation at 10:00     │                             │  prefilled 10:00    │
 │<───────────────────────────────────────────────────────────────────────────│
```

---

## 9. Testing Strategy

### 9.1 Backend Tests

```
tests/FamilyHub.Search.Tests/
├── Domain/
│   └── SearchTermTests.cs                    # Value object validation
├── Application/
│   ├── UnifiedSearchQueryHandlerTests.cs     # Handler logic (parallel providers)
│   └── Models/
│       └── SearchResultItemTests.cs
└── Infrastructure/
    └── Providers/
        ├── CalendarSearchProviderTests.cs     # Calendar search logic
        ├── FamilySearchProviderTests.cs       # Family search logic
        └── FileSearchProviderTests.cs         # File search logic
```

**Example Unit Test:**

```csharp
// tests/FamilyHub.Search.Tests/Application/UnifiedSearchQueryHandlerTests.cs

public class UnifiedSearchQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithMultipleProviders_ReturnsGroupedResults()
    {
        // Arrange
        var calendarProvider = new FakeSearchProvider("calendar", "Calendar", results: [
            new SearchResultItem("1", "Turnen", "Di 15:00", "calendar", "CALENDAR",
                "/calendar", null, 0.9, null),
        ]);
        var fileProvider = new FakeSearchProvider("files", "Files", results: [
            new SearchResultItem("2", "Turnen-Anmeldung.pdf", "2.3 MB", "files", "FILE",
                "/files", null, 0.7, null),
        ]);

        var handler = new UnifiedSearchQueryHandler([calendarProvider, fileProvider]);

        var query = new UnifiedSearchQuery(
            Term: "Turnen",
            UserId: UserId.From(Guid.NewGuid()),
            FamilyId: FamilyId.From(Guid.NewGuid()),
            Categories: null,
            MaxResultsPerCategory: 5
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Groups.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Groups.First().Category.Should().Be("calendar");
        result.Groups.First().Items.Should().ContainSingle()
            .Which.Title.Should().Be("Turnen");
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_OnlySearchesRequestedCategories()
    {
        // Arrange
        var calendarProvider = new FakeSearchProvider("calendar", "Calendar", results: [
            new SearchResultItem("1", "Test", null, "calendar", "CALENDAR",
                "/calendar", null, 0.9, null),
        ]);
        var fileProvider = new FakeSearchProvider("files", "Files", results: [
            new SearchResultItem("2", "Test.pdf", null, "files", "FILE",
                "/files", null, 0.7, null),
        ]);

        var handler = new UnifiedSearchQueryHandler([calendarProvider, fileProvider]);

        var query = new UnifiedSearchQuery(
            Term: "Test",
            UserId: UserId.From(Guid.NewGuid()),
            FamilyId: FamilyId.From(Guid.NewGuid()),
            Categories: ["calendar"], // Only calendar
            MaxResultsPerCategory: 5
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Groups.Should().ContainSingle()
            .Which.Category.Should().Be("calendar");
    }
}
```

### 9.2 Frontend Tests (Playwright E2E)

```typescript
// e2e/search/command-palette.spec.ts

import { test, expect } from '@playwright/test';

test.describe('Command Palette', () => {
  test.beforeEach(async ({ page }) => {
    // Login and navigate to dashboard
    await loginAsTestUser(page);
    await page.goto('/dashboard');
  });

  test('opens with Ctrl+K and closes with Escape', async ({ page }) => {
    await page.keyboard.press('Control+k');
    await expect(page.getByTestId('command-palette-input')).toBeVisible();
    await expect(page.getByTestId('command-palette-input')).toBeFocused();

    await page.keyboard.press('Escape');
    await expect(page.getByTestId('command-palette-input')).not.toBeVisible();
  });

  test('navigates to calendar via command', async ({ page }) => {
    await page.keyboard.press('Control+k');
    await page.getByTestId('command-palette-input').fill('calendar');

    await expect(page.getByTestId('cmd-nav-calendar')).toBeVisible();
    await page.keyboard.press('Enter');

    await expect(page).toHaveURL(/\/calendar/);
  });

  test('shows search results for cross-domain query', async ({ page }) => {
    await page.keyboard.press('Control+k');
    await page.getByTestId('command-palette-input').fill('Turnen');

    // Wait for debounced search
    await expect(page.getByTestId('command-palette-results')).toContainText('Calendar Events');
  });

  test('keyboard navigation works (arrow keys + enter)', async ({ page }) => {
    await page.keyboard.press('Control+k');
    await page.getByTestId('command-palette-input').fill('>');

    // Arrow down to second item
    await page.keyboard.press('ArrowDown');
    await page.keyboard.press('ArrowDown');
    await page.keyboard.press('Enter');

    // Should have navigated somewhere
    await expect(page.getByTestId('command-palette-input')).not.toBeVisible();
  });
});
```

---

## 10. Performance Considerations

### 10.1 Frontend Performance

| Concern | Solution |
|---------|----------|
| **Debouncing** | 300ms debounce on search input to prevent excessive API calls |
| **Bundle size** | Command palette is lazy-loaded only when first triggered |
| **Rendering** | Virtual scrolling for large result sets (> 50 items) |
| **Caching** | Apollo `cache-first` for repeated searches within session |
| **Animation** | CSS-only transitions (no JS animation libraries) |

### 10.2 Backend Performance

| Concern | Solution |
|---------|----------|
| **Parallel execution** | All `ISearchProvider` queries run via `Task.WhenAll` |
| **Result limiting** | `maxResultsPerCategory` caps results per provider (default: 5) |
| **Database indexing** | PostgreSQL GIN indexes on searchable text columns |
| **Timeout** | 2-second timeout per provider (fail gracefully) |
| **RLS** | All queries go through existing RLS middleware (no bypass) |

### 10.3 Recommended Database Indexes

```sql
-- Calendar events: full-text search on title
CREATE INDEX idx_calendar_events_title_trgm
  ON calendar_events USING gin (title gin_trgm_ops);

-- Files: full-text search on name
CREATE INDEX idx_files_name_trgm
  ON files USING gin (name gin_trgm_ops);

-- Family members: search on user name
CREATE INDEX idx_family_members_username_trgm
  ON family_members USING gin (user_name gin_trgm_ops);

-- Enable trigram extension (if not already enabled)
CREATE EXTENSION IF NOT EXISTS pg_trgm;
```

---

## 11. Future Enhancements

### Phase 5+: Advanced Features

1. **Search Analytics** — Track popular searches to improve suggestions
2. **Federated Search** — When migrated to microservices, search via message bus
3. **AI-Powered Intent** — Use LLM for complex natural language understanding
4. **Search Indexing** — Dedicated search index (Elasticsearch/Meilisearch) for scale
5. **Voice Input** — Web Speech API for hands-free family hub interaction
6. **Cross-Family Search** — Admin-level search across families (with proper authorization)
7. **Saved Searches** — Pin frequent searches to sidebar
8. **Search Shortcuts** — User-defined aliases (e.g., "hw" → navigate to homework calendar)

### Module Extension Checklist

When adding a new module's search capability:

1. Create `{Module}SearchProvider` implementing `ISearchProvider`
2. Register it in `SearchModule.Register()`
3. Add repository search methods (e.g., `SearchByTitleAsync`)
4. Add database trigram index for searchable columns
5. Add unit tests for the provider
6. (Optional) Add module-specific commands to `CommandRegistryService`

---

## Summary

The Search & Command Palette is designed to:

- **Respect module boundaries** via the `ISearchProvider` pattern (ADR-001)
- **Follow existing conventions** (IModule, Mediator, GraphQL namespace types, Vogen)
- **Ship incrementally** across 4 phases (commands → search → NLP → smart suggestions)
- **Support bilingual input** (German and English) from day one
- **Enforce permissions** at every layer (RLS, authorization service, frontend hide pattern)
- **Scale gracefully** via parallel provider execution and database indexing
