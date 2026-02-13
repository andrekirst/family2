# Customizable Dashboard with Widget System

## Context

Family Hub needs a customizable dashboard where family members can add, move, resize, and configure widgets on a grid. The dashboard is the main landing page after login and currently shows a simple profile/invitation view. This feature transforms it into a fully extensible widget system where **any module can register its own widgets** — mirroring the existing `IChainPlugin`/`IChainRegistry` pattern from the EventChain module.

**Key decisions:**

- **Fully extensible** widget system via `IWidgetProvider` + `IWidgetRegistry` (mirrors `IChainPlugin`/`IChainRegistry`)
- **gridstack.js** for drag & drop grid layout with resize
- **Both** personal (per-user) and shared (per-family) dashboards
- **All standards** apply (ddd-modules, graphql, permissions, angular-components, apollo, testing)

---

## Task 1: Save Spec Documentation

Create `agent-os/specs/2026-02-13-customizable-dashboard/` with:

- `plan.md` — This full plan
- `shape.md` — Shaping notes (scope, decisions, context)
- `standards.md` — All applicable standards
- `references.md` — IChainRegistry pattern, Family module, frontend architecture
- `database-schema.md` — Dashboard schema design
- `graphql-api.md` — Full GraphQL API definition

---

## Task 2: Widget Infrastructure (Shared Backend)

Create cross-cutting widget registry in `src/FamilyHub.Api/Common/Widgets/`:

| File | Purpose |
|------|---------|
| `IWidgetProvider.cs` | Interface modules implement to declare widgets (mirrors `IChainPlugin`) |
| `WidgetDescriptor.cs` | Record: WidgetTypeId, Module, Name, Description, sizes, ConfigSchema, RequiredPermissions |
| `IWidgetRegistry.cs` | Singleton interface: RegisterProvider, GetAllWidgets, GetWidget, IsValidWidget |
| `WidgetRegistry.cs` | Implementation collecting descriptors from all providers |
| `WidgetRegistryInitializer.cs` | `IHostedService` that collects all `IWidgetProvider` instances at startup |

**Pattern source:** `src/FamilyHub.EventChain.Infrastructure/Registry/ChainRegistry.cs`

---

## Task 3: Dashboard Module — Domain Model

Create `src/FamilyHub.Api/Features/Dashboard/`:

**Value Objects** (`Domain/ValueObjects/`):

- `DashboardId` (Guid, Vogen)
- `DashboardLayoutName` (string, max 100 chars)
- `DashboardWidgetId` (Guid, Vogen)
- `WidgetTypeId` (string, Vogen)

**Entities** (`Domain/Entities/`):

- `DashboardLayout` — Aggregate root. Factory methods: `CreatePersonal(name, userId)`, `CreateShared(name, familyId, createdByUserId)`. Methods: `AddWidget()`, `RemoveWidget()`, `ReplaceAllWidgets()`, `UpdateName()`
- `DashboardWidget` — Entity owned by layout. Fields: WidgetType, X, Y, Width, Height, SortOrder, ConfigJson. Methods: `UpdatePosition()`, `UpdateConfig()`

**Domain Events** (`Domain/Events/`):

- `DashboardCreatedEvent(DashboardId, UserId, IsShared)`

**Repository** (`Domain/Repositories/`):

- `IDashboardLayoutRepository` — GetById, GetPersonal, GetShared, Add, Update, Delete

**Module** (`DashboardModule.cs`):

- Register `IWidgetRegistry` (singleton), `IDashboardLayoutRepository` (scoped), `WidgetRegistryInitializer` (hosted service)
- Add `builder.Services.RegisterModule<DashboardModule>(configuration)` to `Program.cs`

**Pattern source:** `src/FamilyHub.Api/Features/Family/Domain/Entities/Family.cs`

---

## Task 4: Database & Migrations

**EF Core Configurations** (`Features/Dashboard/Data/`):

- `DashboardLayoutConfiguration` — schema `dashboard`, table `dashboard_layouts`
- `DashboardWidgetConfiguration` — schema `dashboard`, table `dashboard_widgets`

**AppDbContext** — Add `DbSet<DashboardLayout>` and `DbSet<DashboardWidget>`

**Migrations:**

1. `AddDashboardSchema` — tables, indexes, foreign keys
2. `AddDashboardRlsPolicies` — RLS with OR logic (personal: user_id match, shared: family_id match)

**RLS policy logic:**

```sql
USING (
  ("user_id"::text = current_setting('app.current_user_id', true))
  OR
  ("family_id"::text = current_setting('app.current_family_id', true))
)
```

---

## Task 5: GraphQL API

**Namespace types:** `DashboardQuery`, `DashboardMutation` (add to `RootQuery`/`RootMutation`)

**Queries:**

| Query | Handler | Returns |
|-------|---------|---------|
| `availableWidgets` | Reads `IWidgetRegistry`, filters by user permissions | `[WidgetDescriptorDto]` |
| `myDashboard` | `GetPersonalDashboardAsync(userId)` | `DashboardLayoutDto?` |
| `familyDashboard` | `GetSharedDashboardAsync(familyId)` | `DashboardLayoutDto?` |

**Mutations:**

| Mutation | Purpose |
|----------|---------|
| `saveLayout` | Bulk save: get-or-create dashboard, validate widget types, `ReplaceAllWidgets()` |
| `addWidget` | Add single widget to existing dashboard |
| `removeWidget` | Remove widget by ID |
| `updateWidgetConfig` | Update widget's configJson |
| `resetDashboard` | Clear all widgets from dashboard |

**Subfolder layout:** `Application/Commands/{Name}/Command.cs, Handler.cs, MutationType.cs, Validator.cs`

**Pattern source:** `src/FamilyHub.Api/Features/Family/Application/Commands/CreateFamily/`

---

## Task 6: Frontend Widget System

**Install:** `npm install gridstack` in `src/frontend/family-hub-web/`

**Core widget infrastructure** (`src/app/core/dashboard/`):

- `dashboard-widget.interface.ts` — `DashboardWidgetComponent` interface with `widgetConfig` signal
- `widget-registry.model.ts` — `WidgetRegistration` type (id, title, component, sizes, permissions)
- `dashboard.tokens.ts` — `DASHBOARD_WIDGET` InjectionToken (multi)
- `widget-registry.service.ts` — Singleton collecting all multi-provided registrations

**Dashboard feature** (`src/app/features/dashboard/`):

- `dashboard.providers.ts` — `provideDashboardFeature()` with built-in widget registrations
- `services/dashboard.service.ts` — Apollo operations wrapper
- `services/dashboard-state.service.ts` — Signals: layout, isEditMode, isLoading, hasUnsavedChanges
- `graphql/dashboard.operations.ts` — GQL queries and mutations

**Modify:** `app.config.ts` — add `...provideDashboardFeature()`

---

## Task 7: Frontend Dashboard Grid

**Components:**

- `widget-container/` — Wraps each widget with header (title, settings gear, remove button), uses `NgComponentOutlet` for dynamic rendering
- `widget-picker/` — Modal showing available widgets from registry, filtered by permissions

**Rewrite `dashboard.component.ts`:**

- Initialize gridstack.js grid with widget positions from API
- Edit mode toggle: enables drag/resize, shows add/remove controls
- Save button: collect positions from gridstack, call `SaveDashboardLayout` mutation
- Default layout for new users (Welcome + Family Overview + Pending Invitations)
- TopBar integration: "Dashboard" title, "Edit Layout" / "Add Widget" actions

**Add gridstack CSS:** `angular.json` styles array

---

## Task 8: Initial Widget Components

| Widget | Location | Data Source |
|--------|----------|-------------|
| `WelcomeWidget` | `widgets/welcome-widget/` | UserService (greeting + quick actions) |
| `FamilyOverviewWidget` | `widgets/family-overview-widget/` | GetMyFamily query (members + roles) |
| `PendingInvitationsWidget` | `widgets/pending-invitations-widget/` | Existing invitation logic extracted |
| `UpcomingEventsWidget` | `widgets/upcoming-events-widget/` | CalendarService (placeholder for now) |

**Backend providers:**

- `DashboardWidgetProvider` — built-in widgets (welcome)
- `FamilyWidgetProvider` — family module widgets (overview, invitations)

---

## Task 9: Tests

**Test project:** `tests/FamilyHub.Dashboard.Tests/` (add to `FamilyHub.slnx`)

**Fake:** `FakeDashboardLayoutRepository` in `tests/FamilyHub.TestCommon/Fakes/`

| Test File | Coverage |
|-----------|----------|
| `DashboardLayoutTests.cs` | Aggregate: Create, AddWidget, RemoveWidget, ReplaceAll, domain events |
| `WidgetRegistryTests.cs` | RegisterProvider, GetAll, GetByModule, IsValid |
| `ValueObjectTests.cs` | DashboardId, DashboardLayoutName, WidgetTypeId validation |
| `SaveDashboardLayoutCommandHandlerTests.cs` | Create new, update existing, reject invalid widget types |
| `AddWidgetCommandHandlerTests.cs` | Add widget, validate type, check permissions |
| `RemoveWidgetCommandHandlerTests.cs` | Remove existing, throw for missing |
| `UpdateWidgetConfigCommandHandlerTests.cs` | Update config JSON |

---

## Task 10: Integration & Polish

- Register `FamilyWidgetProvider` in `FamilyModule.Register()`
- Default dashboard template (frontend-side, no backend templates for V1)
- TopBar integration with edit mode actions
- Solution file update (`FamilyHub.slnx`)

---

## Files Modified (Shared/Existing)

| File | Change |
|------|--------|
| `src/FamilyHub.Api/Program.cs` | Add `RegisterModule<DashboardModule>()` |
| `src/FamilyHub.Api/Common/Database/AppDbContext.cs` | Add 2 DbSet properties |
| `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootQuery.cs` | Add `Dashboard()` |
| `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootMutation.cs` | Add `Dashboard()` |
| `src/FamilyHub.Api/Features/Family/FamilyModule.cs` | Register `FamilyWidgetProvider` |
| `src/FamilyHub.Api/FamilyHub.slnx` | Add test project |
| `src/frontend/family-hub-web/src/app/app.config.ts` | Add `...provideDashboardFeature()` |
| `src/frontend/family-hub-web/angular.json` | Add gridstack CSS |
| `src/frontend/family-hub-web/src/app/features/dashboard/dashboard.component.ts` | Full rewrite |

---

## Verification

1. **Backend:** `dotnet build src/FamilyHub.Api/FamilyHub.slnx` compiles
2. **Tests:** `dotnet test` — all existing 77 tests pass + new dashboard tests
3. **Migration:** `dotnet ef database update` creates dashboard schema with RLS
4. **GraphQL:** Banana Cake Pop shows `dashboard` namespace with queries/mutations
5. **Frontend:** `ng serve` — dashboard loads with default widgets, edit mode enables drag/resize
6. **E2E:** Login → see dashboard with widgets → toggle edit → drag widget → save → refresh → layout persisted
