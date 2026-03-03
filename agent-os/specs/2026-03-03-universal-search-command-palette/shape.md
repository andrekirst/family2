# Universal Search and Command Palette — Shaping Notes

**Feature**: Unified search, command palette, and natural language input for Family Hub
**Created**: 2026-03-03
**GitHub Issue**: #208

---

## Scope

### In Scope

- **Universal Search**: Full-text search across all modules (family members, calendar events, messages, files), scoped by user's family and permissions
- **Command Palette**: Overlay UI (Ctrl+K / Cmd+K) with quick actions like "create event", "invite member" — navigates to the right view
- **Natural Language Parsing**: Rule-based pattern matching for German and English (e.g., "Morgen Termin um 10 Uhr" → create calendar event at specified time)
- **Keyboard Navigation**: Arrow keys, Enter to execute, Escape to close, focus trap
- **Module Provider Pattern**: Extensible — each module registers its own search provider and command provider

### Out of Scope

- AI/LLM-powered parsing (rule-based only)
- Full-text search index (e.g., Elasticsearch) — not needed at family-scale
- Search result caching/pagination (simple limit-based for now)
- File content search (metadata only)
- Voice input

## Decisions

### Q: Should search be its own module or a cross-cutting concern?

**A: Hybrid.** `Common/Search/` holds the interfaces (`ISearchProvider`, `ICommandPaletteProvider`, registries) — this is the cross-cutting contract. `Features/Search/` is a new module owning the GraphQL endpoint and orchestration handler. This mirrors the proven `Common/Widgets/` + `Features/Dashboard/` pattern.

### Q: Real-time fanout vs. dedicated search index?

**A: Real-time fanout.** Each `ISearchProvider` queries its own module's data. At family scale (tens to hundreds of records per module), `ILIKE` or `pg_trgm` is fast enough. `Task.WhenAll` parallelizes across providers. If performance needs grow, `tsvector` columns can be added behind the same interface.

### Q: Where does NLP parsing happen?

**A: Frontend (TypeScript).** Zero-latency parsing as user types. The locale is already client-side. Backend only receives structured GraphQL queries. Rules are deterministic regex patterns — microsecond evaluation.

### Q: How do modules register their searchable content?

**A: Provider pattern (same as Widgets).** Each module implements `ISearchProvider` and/or `ICommandPaletteProvider` and registers them in its `IModule.Register()`. A `SearchRegistryInitializer` (`IHostedService`) wires providers at startup. The search handler resolves `IEnumerable<ISearchProvider>` and fans out.

### Q: Command vs. search mode in the UI?

**A: Unified input.** No separate modes. Results are grouped by type: NLP suggestion (if matched) at top, then search results grouped by module, then matching commands. No ">" prefix for command mode — everything flows naturally from the same input.

## Context

- **Visuals**: 4 Dribbble reference images showing centered command palette overlays with grouped results:
  - https://cdn.dribbble.com/userupload/35497075/file/original-0b58551936814d239aa32b67ec6aa9b4.png
  - https://cdn.dribbble.com/userupload/41560267/file/original-90b8712f20e8c684df02ecdae80b3daa.jpg
  - https://cdn.dribbble.com/userupload/12779867/file/original-1f4c7b529953b29d7e8a2727130d263c.jpg
  - https://cdn.dribbble.com/userupload/11284127/file/original-19187f94751fa67962d504aec9fd0520.png
- **References**: No existing search functionality in codebase. Widget provider/registry pattern is the closest architectural reference.
- **Product alignment**: N/A (no product folder)

## Standards Applied

- **architecture/ddd-modules** — New `SearchModule` follows modular monolith pattern
- **backend/graphql-input-command** — `UniversalSearchRequest` (Input) → `UniversalSearchQuery` (Query) with Vogen types
- **backend/user-context** — `IUserService` + `ClaimNames.Sub` for scoping search to current user
- **backend/permission-system** — Commands filtered by user permissions before returning
- **frontend/angular-components** — Standalone component with signals, Tailwind styling
- **frontend/apollo-graphql** — Apollo `query()` with `network-only` fetch policy for fresh results

## Risks & Mitigations

| Risk | Mitigation |
|------|-----------|
| Search performance with many modules | `Task.WhenAll` parallelization + per-provider result limits |
| NLP false positives | Confidence threshold (>0.5) and explicit "Smart Suggestion" label |
| Keyboard shortcut conflicts | Ctrl+K is universal standard; only active when no input focused |
| Provider lifecycle mismatch | Registries are Singleton; providers resolved Scoped in handler via DI |

## Success Indicators

- **Functional**: Users can find content across modules from a single input
- **Quality**: <300ms response time for search queries
- **UX**: Ctrl+K opens palette, results appear grouped, keyboard navigation works end-to-end
- **Extensibility**: New module search providers can be added with 2 files + 1 DI line
