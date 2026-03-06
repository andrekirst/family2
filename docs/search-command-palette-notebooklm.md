# Issue #208: Universal Search & Command Palette — Complete Implementation Guide

## Executive Summary

The Universal Search & Command Palette is a feature for the Family Hub application that gives users a single keyboard shortcut (Ctrl+K) to search across all modules, execute commands, navigate the app, and use natural language phrases. It combines three data sources: client-side NLP parsing, server-side GraphQL search across 8 modules, and static default items with daily-rotating hints. The implementation spans the full stack from PostgreSQL queries through .NET 9 GraphQL to an Angular 19 modal component.

---

## 1. The Big Picture — What This Feature Does

When a user presses Ctrl+K anywhere in the Family Hub app, a modal overlay appears with a search input. The palette serves four purposes:

1. **Discovery** — When the input is empty, it shows 8 default items: 2 NLP hints (rotated daily), 2 quick actions, and 4 navigation shortcuts. This teaches users what they can do.

2. **Natural Language Understanding** — The frontend parses what the user types using regex patterns. Typing "tomorrow event at 3 PM" creates a calendar event suggestion with the date and time pre-filled. This works entirely client-side with no server call.

3. **Full-Text Search** — The query is also sent to the backend via GraphQL, where 8 module-specific search providers query PostgreSQL for matching family members, calendar events, files, messages, photos, automations, and dashboards.

4. **Command Discovery** — The backend also returns matching commands from a registry of 22 actions (like "Invite Member", "Create Folder", "Upload Photo"), filtered by the user's permissions.

All results are merged into a unified list with section headers, emoji icons, keyboard navigation, and instant routing when an item is selected.

---

## 2. Architecture Layers

The system has five distinct layers:

### Layer 1: User Interface (Angular Component)

The `CommandPaletteComponent` is a standalone Angular 19 component placed at the root layout level. It renders a fixed-position modal with backdrop blur, search input, results list, and keyboard hint footer. It uses Angular signals for reactive state management.

### Layer 2: Orchestration Service (Angular Service)

The `CommandPaletteService` manages all state (open/closed, query, items, loading, error) and coordinates the three data sources. It handles debouncing (300ms), default item generation, and the special hint-click behavior.

### Layer 3: Client-Side Intelligence (NLP Parser)

The `NlpParserService` runs 17 regex patterns against the user's query, supporting both English and German. It includes date parsing ("tomorrow", "friday", "morgen") and time parsing ("3 PM", "15 Uhr"). This layer is completely independent of the server.

### Layer 4: GraphQL API (Hot Chocolate)

The `QueryType` resolver handles authentication, extracts the user's family membership and permissions from JWT claims, and dispatches a `UniversalSearchQuery` through the Mediator pipeline.

### Layer 5: Data Collection (Search Providers + Command Registry)

Eight `ISearchProvider` implementations query PostgreSQL through Entity Framework Core. A `CommandPaletteRegistry` singleton caches 22 commands from 8 `ICommandPaletteProvider` implementations, populated at application startup by an `IHostedService`.

---

## 3. Data Flow — Step by Step

### Step 1: Opening the Palette

The user presses Ctrl+K. The component captures this via a `@HostListener('document:keydown')`. The service sets `isOpen = true`, resets the query, and populates items with `getDefaultItems()`. The input is auto-focused via an Angular `effect()`.

### Step 2: Default Items (Empty Query)

When the query is empty, 8 static items are shown immediately with no network request:

- **2 NLP Hints** (type: `hint`): Selected from a pool of 10 per locale using deterministic daily rotation. The algorithm uses `Math.floor(Date.now() / 86_400_000) % poolSize` — epoch-day modular arithmetic. Two consecutive indices are used so users see two different hints each day. The pool includes phrases like "tomorrow event at 3 PM", "invite john@example.com", "open calendar".

- **2 Quick Actions** (type: `command`): "Create Event" routing to `/family/calendar?action=create` and "Send Message" routing to `/messages?action=create`.

- **4 Navigation Items** (type: `navigation`): Dashboard, Calendar, Messages, Files — each with a specific route and module-specific emoji icon.

### Step 3: User Types a Query

As the user types, `onQueryChange()` is called. After 300ms of inactivity (debounce), `performSearch()` is triggered. Two things happen in parallel:

**Client-side NLP parsing:** The `NlpParserService` tries each regex pattern against the query. If a pattern matches and the confidence exceeds 0.5, it returns an `NlpMatch` with a pre-built route. For example, "tomorrow event at 3 PM" would match the calendar event pattern, parse "tomorrow" to a date and "3 PM" to 15:00, and produce the route `/family/calendar?action=create&date=2026-03-06&time=15:00` with 0.9 confidence.

**Server-side GraphQL search:** The `SearchService` sends an Apollo query to the `search.universal` endpoint. The request includes the query string, optional module filter, result limit, and the user's locale.

### Step 4: Backend Processing

The GraphQL `QueryType` resolver:

1. Extracts the user ID from the JWT `sub` claim
2. Looks up the user and resolves their family membership
3. Extracts permissions from their family role (Owner > Admin > Member hierarchy)
4. Dispatches `UniversalSearchQuery` with all resolved context

The `UniversalSearchQueryHandler`:

1. Filters search providers by requested modules (if specified)
2. Builds a `SearchContext` with userId, familyId, query, limit, and locale
3. Executes each provider sequentially (they share a scoped DbContext)
4. Catches errors per provider — if one fails, the others continue
5. Flattens all results and caps at 30 total
6. Filters commands by keyword match and permission check
7. Resolves German labels if locale is "de"

### Step 5: Results Assembly

Back in the frontend, the service assembles the final `PaletteItem[]` array in this order:

1. NLP suggestions (type: `nlp`, max 1 item)
2. Search results from GraphQL (type: `result`, up to 30 items)
3. Commands from GraphQL (type: `command`, filtered subset of 22)

### Step 6: Rendering

The component renders items grouped by consecutive type values. Each group gets a section header:

- `hint` → "Try saying..."
- `nlp` → "Suggestions"
- `result` → "Search Results"
- `command` → "Commands"
- `navigation` → "Navigation"

Each item shows an emoji icon (type-specific or module-specific), title, optional description, optional module badge, and an "Enter" hint on the selected item.

### Step 7: Item Execution

When the user clicks or presses Enter on an item:

- **Hint items:** Instead of navigating, the hint text fills the search input and triggers a search. This teaches users how NLP works by demonstrating it live.
- **All other items:** The palette closes and `router.navigateByUrl(item.route)` navigates to the target.

---

## 4. Search Providers — What Each Module Searches

### Family Module (FamilySearchProvider)

Searches the family name, family members by name or email, and pending invitations. Requires a family context (returns empty if user has no family). Provides 3 commands: View Family, Invite Member (requires `family:invite` permission), Family Settings.

### Calendar Module (CalendarSearchProvider)

Searches calendar events within a 9-month window: 3 months in the past and 6 months in the future. Sorts results with upcoming events first. Provides 2 commands: Create Event, View Calendar.

### File Management Module (FileManagementSearchProvider)

The most comprehensive provider. Searches files (by name), folders (by name), albums (by name), tags (by name), secure notes (category only — content is encrypted), and share links. Provides 8 commands covering Create Folder, Upload File, Create Album, Browse Files, View Albums, Manage Tags, Create Secure Note, View Secure Notes.

### Messaging Module (MessagingSearchProvider)

Searches messages by content, ordered newest first. Provides 2 commands: New Message, View Messages.

### Dashboard Module (DashboardSearchProvider)

Searches personal and family dashboards. Provides 1 command: Open Dashboard.

### Event Chain Module (EventChainSearchProvider)

Searches automation workflows by name and description, includes enabled/disabled status in results. Provides 2 commands: Create Chain, View Chains.

### Photos Module (PhotosSearchProvider)

Searches photos by filename and caption, ordered by creation date (newest first). Provides 2 commands: Upload Photo, View Photos.

### Auth Module (No SearchProvider)

Does not contribute search results, but provides 2 commands via `ProfileCommandPaletteProvider`: View Profile, Settings.

---

## 5. NLP Rules — Natural Language Understanding

The NLP system uses 17 regex patterns per locale (English and German). Patterns are tried in order; the best match (highest confidence above 0.5) wins.

### Calendar Event Creation (Confidence: 0.65–0.9)

- English: "tomorrow event at 3 PM", "event friday at 10 AM", "meeting today"
- German: "morgen Termin um 15 Uhr", "Termin Freitag um 10 Uhr"
- Parses relative dates: today, tomorrow, day names (finds next occurrence)
- Parses times: 12-hour ("3 PM") and 24-hour ("15 Uhr") formats
- Route includes pre-filled date and time query parameters

### Navigation (Confidence: 0.85)

- English: "go to dashboard", "open calendar", "show messages", "view files"
- German: "zum Dashboard gehen", "Kalender öffnen", "Nachrichten zeigen"
- Covers 10 destinations: Dashboard, Calendar, Messages, Files, Photos, Albums, Family, Automations, Profile, Settings

### Actions (Confidence: 0.7–0.9)

- Invite with email: "invite john@example.com" → pre-fills email
- Invite generic: "invite member" → opens invite dialog
- Create folder: "create folder Vacation" → pre-fills name
- Create album: "create album Summer" → pre-fills name
- Send message: "send a message" → opens compose
- Search files: "find files report" → file search with query
- Upload: "upload a file" → opens upload dialog

### Date Parsing

Supports relative dates in both languages:

- "today"/"heute" → current date
- "tomorrow"/"morgen" → next day
- "übermorgen" (German only) → day after tomorrow
- Day names in both languages → next occurrence of that weekday

### Time Parsing

- English: "3 PM" → "15:00", "10:30 AM" → "10:30"
- German: "15 Uhr" → "15:00", "10:30 Uhr" → "10:30"

---

## 6. Command Palette Registry — Startup Architecture

At application startup, the system uses an `IHostedService` called `CommandPaletteRegistryInitializer` to populate the command registry:

1. `Program.cs` registers all feature modules via `RegisterModule<T>()`
2. Each module registers its `ICommandPaletteProvider` as a singleton and its `ISearchProvider` as scoped
3. The `SearchModule` registers the `CommandPaletteRegistry` as a singleton
4. At startup, `CommandPaletteRegistryInitializer` iterates all `ICommandPaletteProvider` instances via dependency injection and calls `registry.RegisterProvider(provider)` for each
5. This populates the registry with all 22 commands from 8 modules
6. The registry is then available as a singleton for the lifetime of the application

This pattern means commands are cached once at startup with zero runtime overhead. Search providers, by contrast, are scoped per-request because they need database access.

---

## 7. Internationalization (i18n) Strategy

The system supports English and German at every layer:

### Frontend

- `LOCALE_ID` injection determines which NLP rules and hint pool to use
- Separate rule files: `en.rules.ts` and `de.rules.ts` with equivalent patterns
- Date parser handles "tomorrow"/"morgen", day names in both languages
- Time parser handles "3 PM" (12-hour) and "15 Uhr" (24-hour)
- Hint pool: 10 English phrases, 10 German phrases

### Backend

- `CommandDescriptor` has optional `LabelDe` and `DescriptionDe` fields
- Keywords arrays include both English and German terms (e.g., `["invite", "einladen", "member", "mitglied"]`)
- `UniversalSearchQuery` carries a `Locale` parameter
- `UniversalSearchQueryHandler.ResolveGermanLabels()` swaps in German labels when locale starts with "de"
- Search providers check locale for description text

---

## 8. Error Resilience Design

The system is designed to degrade gracefully at every level:

- **NLP Parser failure:** Caught in a try/catch. If NLP parsing throws, the GraphQL search still runs and results are shown without NLP suggestions.

- **Individual search provider failure:** Each provider's `SearchAsync()` call is wrapped in try/catch in the query handler. If the Calendar provider throws, the other 7 providers still return their results. The error is logged with the provider name and query.

- **GraphQL request failure:** The `SearchService` returns an empty result `{results: [], commands: []}`. The palette shows a "Search failed" error message.

- **Authentication failure:** The GraphQL query is decorated with `[Authorize]`. Unauthenticated requests receive a standard GraphQL auth error.

---

## 9. UI Component Design

### Modal Structure

- Fixed position overlay at z-index 50
- Semi-transparent backdrop with blur effect
- Centered modal at 15% from top, max-width 36rem
- Rounded corners, shadow, white background

### Search Input Area

- Search magnifying glass icon
- Text input with placeholder "Search or type a command..."
- Loading indicator (3 animated dots) during GraphQL requests
- Escape key hint badge

### Results List

- Scrollable area with max height 320px
- Items grouped by type with uppercase section headers
- Each item: 32x32 icon area, title (truncated), optional description, optional module badge
- Selected item: blue highlight, "Enter" keyboard hint
- Hover changes selection

### Footer

- Only visible when items are present
- Keyboard hints: ↑↓ navigate, Enter select, Esc close

### Accessibility

- `role="dialog"` with `aria-modal="true"` and `aria-label`
- `role="listbox"` on results with `aria-activedescendant`
- `role="option"` on each item with `aria-selected`
- Focus management: saves previous element, auto-focuses input, restores on close
- Tab trapping prevents focus from escaping the modal

---

## 10. Key Technical Decisions

### Why Client-Side NLP?

Natural language parsing runs on the client using regex patterns rather than sending text to a server-side NLP service. This provides instant feedback (no network latency), works offline, and keeps the implementation simple. The confidence scoring system (0-1 scale with 0.5 threshold) prevents false positives.

### Why Sequential Provider Execution?

Search providers execute sequentially rather than in parallel because they share a scoped Entity Framework DbContext. Parallel execution would require separate DbContext instances per provider, adding complexity. The sequential approach is fast enough because each provider's query is simple and indexed.

### Why Singleton Command Registry?

Commands are static metadata (label, description, route, permissions). They never change at runtime, so caching them once at startup eliminates repeated DI resolution and list construction on every request.

### Why Daily Hint Rotation?

Using epoch-day math for hint selection means all users see the same hints on the same day (predictable), hints change daily (fresh), and no storage or state management is needed. The algorithm is deterministic and requires zero configuration.

### Why Separate Hint Click Behavior?

When users click an NLP hint, instead of navigating directly, the hint text fills the search input and triggers NLP parsing. This teaches users the natural language syntax by demonstrating it live — a progressive disclosure pattern that reduces the learning curve.

---

## 11. Data Models Reference

### PaletteItem (Frontend — Unified Display Model)

```
type: 'nlp' | 'result' | 'command' | 'hint' | 'navigation'
title: string
description?: string
icon: string
route: string
module?: string
confidence?: number
```

### SearchResultItem (Backend — Provider Output)

```
Title: string
Description?: string
Module: string (e.g., "family", "calendar")
Icon: string (e.g., "users", "calendar")
Route: string (e.g., "/family/members/abc-123")
Metadata?: Dictionary<string, string>
```

### CommandDescriptor (Backend — Registry Entry)

```
Label: string (English)
Description: string (English)
Keywords: string[] (mixed language)
Route: string
RequiredPermissions: string[]
Icon: string
Group: string (module name)
LabelDe?: string (German)
DescriptionDe?: string (German)
```

### SearchContext (Backend — Provider Input)

```
UserId: UserId
FamilyId?: FamilyId
Query: string
Limit: int (default 10)
Locale?: string
```

### NlpMatch (Frontend — Parser Output)

```
type: 'create-event' | 'navigate' | 'invite-member' | 'create-folder' | 'create-album' | 'send-message' | 'search-files'
confidence: number (0-1)
title?: string
date?: Date
time?: string (HH:MM)
route: string
description: string
```

---

## 12. File Structure

### Backend Files

- `src/FamilyHub.Api/Common/Search/` — Core interfaces and models (ISearchProvider, ICommandPaletteProvider, ICommandPaletteRegistry, CommandDescriptor, SearchContext, SearchResultItem, CommandPaletteRegistry, CommandPaletteRegistryInitializer)
- `src/FamilyHub.Api/Features/Search/` — Search module, GraphQL query type, handler, request/result models
- `src/FamilyHub.Api/Features/{Module}/Application/Search/` — Per-module search provider and command palette provider implementations
- `tests/FamilyHub.Search.Tests/` — 12 unit tests for the query handler

### Frontend Files

- `src/app/core/nlp/` — NLP parser service, models, and locale-specific rules (en.rules.ts, de.rules.ts, date-parser.ts, time-parser.ts)
- `src/app/shared/models/search.models.ts` — TypeScript interfaces for PaletteItem, SearchResultItem, CommandDescriptor
- `src/app/shared/graphql/search.operations.ts` — GraphQL query definition
- `src/app/shared/services/search.service.ts` — Apollo GraphQL client wrapper
- `src/app/shared/services/command-palette.service.ts` — State management and orchestration
- `src/app/shared/components/command-palette/command-palette.component.ts` — UI component with template
- `src/app/shared/layout/layout.component.ts` — App shell that hosts the palette

---

## 13. Technology Stack Used

- **Backend:** .NET 9, C# 13, Hot Chocolate (GraphQL), martinothamar/Mediator (CQRS), Entity Framework Core, PostgreSQL, Vogen (value objects)
- **Frontend:** Angular 19, TypeScript, Angular Signals, Apollo Client (GraphQL), Tailwind CSS
- **Auth:** Keycloak (OAuth 2.0 / OIDC), JWT claims
- **Testing:** xUnit, FluentAssertions, NullLogger for test doubles

---

## 14. Performance Characteristics

- **Default items:** Zero latency — computed locally from static data
- **NLP parsing:** Sub-millisecond — 17 regex patterns tested against a short string
- **GraphQL search:** Network-dependent — typically 50-200ms including 8 provider queries
- **Debouncing:** 300ms delay prevents excessive API calls during typing
- **Command registry:** Zero per-request cost — populated once at startup as singleton
- **Result cap:** Maximum 30 search results prevents overwhelming the UI
- **Focus management:** Uses `setTimeout(0)` for DOM-ready focus after Angular renders the input

---

## 15. Summary Diagram Description

Imagine the system as three concentric rings:

The **outer ring** is the Angular UI — the modal component that the user sees and interacts with. It captures keyboard input, renders items with section headers and emoji icons, and navigates when items are selected.

The **middle ring** is the orchestration layer — the `CommandPaletteService` that decides which data source to use (defaults for empty queries, NLP + GraphQL for typed queries), manages debouncing and loading states, and normalizes everything into `PaletteItem[]`.

The **inner ring** is the data layer — split between client-side NLP (regex patterns producing routes with pre-filled parameters) and server-side GraphQL (8 search providers querying PostgreSQL, plus a cached command registry filtered by permissions and locale).

All three rings work together to provide a unified, keyboard-first search experience that works across the entire application, supports two languages, degrades gracefully on errors, and teaches users about NLP capabilities through interactive hints.
