# Customizable Dashboard — Shaping Notes

**Feature**: Customizable dashboard with extensible widget system and grid-based drag & drop
**Created**: 2026-02-13

---

## Scope

As a family member, I want a customizable dashboard where I can add, move, resize, and configure widgets on a grid. The dashboard is the main landing page after login and currently shows a simple profile/invitation view. This feature transforms it into a fully extensible widget system where **any module can register its own widgets**.

### What's In Scope (V1)

1. **Widget Infrastructure**: `IWidgetProvider` + `IWidgetRegistry` (mirrors `IChainPlugin`/`IChainRegistry`)
2. **Dashboard Module**: Domain model with `DashboardLayout` aggregate + `DashboardWidget` entities
3. **Database**: `dashboard` schema with RLS (personal + shared dashboard support)
4. **GraphQL API**: Queries (available widgets, my dashboard, family dashboard) + Mutations (save layout, add/remove/configure widgets)
5. **Frontend Grid**: gridstack.js integration with drag, drop, resize
6. **Widget Container**: Dynamic component rendering via `NgComponentOutlet`
7. **Widget Picker**: Modal to browse and add available widgets
8. **Initial Widgets**: Welcome, Family Overview, Pending Invitations, Upcoming Events (placeholder)
9. **Dual Ownership**: Personal dashboards (per user) + Shared family dashboards
10. **Edit Mode**: Toggle between view and edit modes (move/resize/add/remove)

### What's Out of Scope (Future)

1. Widget-to-widget communication (data passing between widgets)
2. Widget marketplace/sharing between families
3. Real-time collaborative editing of shared dashboards
4. Widget animations/transitions beyond gridstack defaults
5. Mobile-specific dashboard layouts (responsive but not separate layout)
6. Dashboard templates/presets beyond the default layout
7. Widget data refresh intervals (manual refresh only for V1)
8. Dashboard export/import

---

## Decisions

### 1. Widget Registration Pattern

**Question**: How do modules declare their widgets?

**Answer**: **IWidgetProvider + IWidgetRegistry** (mirror IChainPlugin/IChainRegistry)

- Each module implements `IWidgetProvider` to declare its widgets
- `IWidgetRegistry` is a singleton collecting all providers at startup
- `WidgetRegistryInitializer` (IHostedService) populates registry after DI is built
- `WidgetDescriptor` record holds metadata (id, sizes, permissions, config schema)

**Rationale**: IChainPlugin already solved this exact problem. Separate provider interface respects Interface Segregation (not all modules have widgets). Singleton registry acts as cross-cutting discovery service.

### 2. Grid Layout Library

**Question**: How to implement drag & drop grid layout?

**Answer**: **gridstack.js**

- Mature library (used by Grafana)
- Drag, resize, responsive breakpoints out of the box
- ~30KB gzipped
- Good Angular integration via AfterViewInit lifecycle

**Rationale**: CDK Drag & Drop requires too much custom grid logic. gridstack.js is battle-tested for exactly this use case.

### 3. Dashboard Ownership

**Question**: Personal, shared, or both?

**Answer**: **Both**

- Personal dashboard: scoped to `user_id`, fully customizable by each member
- Shared family dashboard: scoped to `family_id`, editable by admins/owners
- RLS uses OR logic to serve both types
- Default view is personal; shared is accessible via toggle

**Rationale**: Personal dashboards respect individual preferences. Shared dashboards enable family coordination (e.g., shared calendar widget visible to all).

### 4. Layout Persistence

**Question**: How to persist widget positions?

**Answer**: **Bulk save (SaveDashboardLayout mutation)**

- Frontend collects all widget positions from gridstack after editing
- Single mutation replaces all widgets atomically
- No individual position update mutations (reduces complexity)
- `ReplaceAllWidgets()` on the aggregate

**Rationale**: Gridstack moves multiple widgets simultaneously (auto-rearrange). Saving individual positions would require tracking which widgets moved. Bulk save is simpler and atomic.

### 5. Widget Configuration

**Question**: How do widgets store their settings?

**Answer**: **JSON column (configJson)**

- Each widget instance has an optional `configJson` column (JSONB in PostgreSQL)
- Widget descriptor declares a `ConfigSchema` (JSON Schema) for validation
- Frontend widget components read config via signal input
- Example: "show next 5 events" vs "show next 10 events"

**Rationale**: JSON is flexible enough for any widget's config without schema migrations. JSON Schema provides validation without coupling.

### 6. Default Dashboard

**Question**: What do new users see on first visit?

**Answer**: **Frontend-generated default layout**

- When `GetMyDashboard` returns null, frontend creates a default layout
- Default: Welcome widget (full width) + Family Overview (left) + Pending Invitations (right)
- Saved on first edit (lazy creation)
- No backend template system for V1

**Rationale**: Keep it simple. Templates can be added later if needed.

### 7. Frontend Widget Registry

**Question**: How are widgets registered on the frontend?

**Answer**: **InjectionToken with multi providers**

- `DASHBOARD_WIDGET` InjectionToken accepts `WidgetRegistration[]`
- Each feature's `provideXxxFeature()` function registers its widgets via `multi: true`
- `WidgetRegistryService` collects all registrations at startup
- Mirrors the backend pattern on the Angular side

**Rationale**: Angular's multi-provider pattern is the idiomatic way to register plugins. Feature providers already exist for each module.

---

## Technical Constraints

1. **Must follow IModule pattern**: Dashboard is a new module registered in Program.cs
2. **Single AppDbContext**: Not per-module — add DbSets to existing context
3. **Hot Chocolate GraphQL**: Namespace types (DashboardQuery/DashboardMutation)
4. **Mediator handlers**: martinothamar/Mediator with ValueTask returns
5. **Vogen value objects**: DashboardId, WidgetTypeId, etc.
6. **RLS on all tables**: Family + user scoped isolation
7. **Angular 19 signals**: No RxJS for state management
8. **Standalone components**: No NgModules

---

## Context

- **Visuals**: Grid-based layout with movable widgets, configurable parameters per widget. No specific mockup — design follows existing Tailwind card patterns.
- **References**: IChainRegistry pattern (EventChain module), Family module (CQRS/GraphQL patterns), existing dashboard component
- **Product alignment**: Dashboard explicitly in Phase 2 backlog (RICE 44.0, P0). Widget types listed: Today's Schedule, Pending Tasks, Shopping List Preview, Family Activity Feed.

## Standards Applied

- **ddd-modules** — Dashboard as a new bounded context with its own schema
- **graphql-input-command** — Input DTOs with primitives, Commands with Vogen types
- **permission-system** — Widget-level permissions, hide unauthorized widgets
- **domain-events** — DashboardCreatedEvent raised on aggregate creation
- **ef-core-migrations** — Dashboard schema with separate migration
- **angular-components** — Standalone widgets with signals
- **apollo-graphql** — Typed operations for dashboard queries/mutations
- **unit-testing** — Fake repository pattern, aggregate tests, handler tests
