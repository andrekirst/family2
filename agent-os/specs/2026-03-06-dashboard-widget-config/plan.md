# Dashboard Widget Configuration (Welcome & Upcoming Events)

**Created**: 2026-03-06
**GitHub Issue**: #218
**Spec**: `agent-os/specs/2026-03-06-dashboard-widget-config/`

## Context

The dashboard has 4 registered widgets but none use the existing `configJson` / `ConfigSchema` infrastructure. Users cannot personalize widget behavior. This feature adds per-widget configuration to the **Welcome Widget** and **Upcoming Events Widget**, using the gear-icon settings pattern in the widget container header.

**User Story**: As a family member, I want to persist my dashboard with configurable widgets so that I see a personalized greeting and customized event view.

## Widget Config Schemas

### Welcome Widget (`dashboard:welcome`)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `greetingText` | string | `"Hello"` | Custom greeting prefix |
| `useTimeBasedGreeting` | boolean | `false` | Override with "Good morning/afternoon/evening" |

### Upcoming Events Widget (`family:upcoming-events`)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `daysIntoFuture` | number | `7` | Days to look ahead (1-90) |
| `maxEntries` | number | `5` | Max events displayed (1-20) |
| `showAllFamilyEvents` | boolean | `true` | All family vs only my events |
| `showEventTime` | boolean | `true` | Show time alongside title |
| `viewMode` | `"compact"` \| `"detailed"` | `"compact"` | List vs card layout |

## Design Decisions

- **Config UI**: Gear icon in widget header (edit mode only), opens settings panel between header and body
- **ConfigSchema**: Informational JSON only (not full JSON Schema validation) -- sufficient for 2 widgets
- **firstName extraction**: `user.name.split(' ')[0]` from `UserService.currentUser()`
- **Upcoming Events mock data**: Hardcoded sample events with "Sample data" badge until Calendar module exists
- **Config persistence**: Immediate save via existing `UpdateWidgetConfig` mutation (not deferred to layout save)

## Implementation Tasks

### Task 1: Save Spec, Commit, and Create GitHub Issue

1. Write spec files to `agent-os/specs/2026-03-06-dashboard-widget-config/`
2. Create GitHub issue with labels: `type-feature`, `status-planning`, `phase-1`, `service-frontend`, `priority-p2`, `effort-m`
3. Update spec files with issue number
4. Git commit: `docs(spec): add dashboard-widget-config spec (#N)`

### Task 2: Backend -- Add ConfigSchema to Widget Providers

**Files to modify:**

- `src/FamilyHub.Api/Features/Dashboard/DashboardWidgetProvider.cs` -- Set `ConfigSchema` JSON for `dashboard:welcome`
- `src/FamilyHub.Api/Features/Family/FamilyWidgetProvider.cs` -- Set `ConfigSchema` JSON for `family:upcoming-events`

Add informational JSON schema strings describing each widget's config properties with types, defaults, and labels. No other backend changes needed -- the `UpdateWidgetConfigCommand` and GraphQL mutation already work end-to-end.

### Task 3: Frontend -- Add WidgetConfigField Type and Registration

**Files to modify:**

- `src/frontend/family-hub-web/src/app/core/dashboard/widget-registry.model.ts` -- Add `WidgetConfigField` interface and optional `configFields` to `WidgetRegistration`
- `src/frontend/family-hub-web/src/app/features/dashboard/dashboard.providers.ts` -- Add `configFields` arrays to `dashboard:welcome` and `family:upcoming-events` registrations

```typescript
export interface WidgetConfigField {
  key: string;
  label: string;
  type: 'string' | 'number' | 'boolean' | 'select';
  default: unknown;
  options?: { value: string; label: string }[];
  min?: number;
  max?: number;
}
```

### Task 4: Frontend -- Widget Settings Panel Component

**New file:**

- `src/frontend/family-hub-web/src/app/features/dashboard/components/widget-settings-panel/widget-settings-panel.component.ts`

Generic config form component that:

- Takes `configFields: WidgetConfigField[]` and current `config` as inputs
- Renders appropriate form controls per field type (text, number, checkbox, select)
- Emits `configSaved` with new config object and `configCancelled`
- Pre-populates defaults when config is null
- Styled with Tailwind, consistent with existing components

### Task 5: Frontend -- Gear Icon + Settings Panel in Widget Container

**File to modify:**

- `src/frontend/family-hub-web/src/app/features/dashboard/components/widget-container/widget-container.component.ts`

Changes:

1. Add `showSettings = signal(false)` state
2. Add gear icon button in header (edit mode, only if widget has `configFields`)
3. Toggle `<app-widget-settings-panel>` between header and body
4. On `configSaved`: call `DashboardService.updateWidgetConfig()`, update widget's `configJson`, pass new config to widget instance via `componentRef.instance.widgetConfig.set(newConfig)`

**File to modify (add mutation):**

- `src/frontend/family-hub-web/src/app/features/dashboard/graphql/dashboard.operations.ts` -- Add `UPDATE_WIDGET_CONFIG` mutation
- `src/frontend/family-hub-web/src/app/features/dashboard/services/dashboard.service.ts` -- Add `updateWidgetConfig()` method

### Task 6: Frontend -- Welcome Widget Config-Driven Behavior

**File to modify:**

- `src/frontend/family-hub-web/src/app/features/dashboard/widgets/welcome-widget/welcome-widget.component.ts`

Changes:

1. Add `computed` signal for greeting: if `useTimeBasedGreeting`, compute "Good morning" (5-11h) / "Good afternoon" (12-16h) / "Good evening" (17-4h); else use `greetingText` (default "Hello")
2. Add `computed` for firstName: `user.name.split(' ')[0]`
3. Update template: `{{ greeting() }}, {{ firstName() }}!`

### Task 7: Frontend -- Upcoming Events Widget Config-Driven Behavior

**File to modify:**

- `src/frontend/family-hub-web/src/app/features/dashboard/widgets/upcoming-events-widget/upcoming-events-widget.component.ts`

Changes:

1. Add computed signals deriving config with defaults
2. Define static mock events array (8-10 entries with future dates)
3. Filter by `daysIntoFuture`, slice to `maxEntries`
4. Render compact (single-line list) or detailed (card with description) based on `viewMode`
5. Conditionally show event time based on `showEventTime`
6. Show "Sample data" badge to indicate mock data
7. Show "No upcoming events" when filtered list is empty

### Task 8: Tests

**Files to modify/create:**

- `tests/FamilyHub.Dashboard.Tests/` -- Test that `DashboardWidgetProvider.GetWidgets()` returns non-null `ConfigSchema` for `dashboard:welcome`
- `tests/FamilyHub.Family.Tests/` -- Test that `FamilyWidgetProvider.GetWidgets()` returns non-null `ConfigSchema` for `family:upcoming-events`

## Files Summary

| File | Action | Task |
|------|--------|------|
| `src/FamilyHub.Api/Features/Dashboard/DashboardWidgetProvider.cs` | Modify | 2 |
| `src/FamilyHub.Api/Features/Family/FamilyWidgetProvider.cs` | Modify | 2 |
| `src/frontend/.../core/dashboard/widget-registry.model.ts` | Modify | 3 |
| `src/frontend/.../features/dashboard/dashboard.providers.ts` | Modify | 3 |
| `src/frontend/.../dashboard/components/widget-settings-panel/widget-settings-panel.component.ts` | Create | 4 |
| `src/frontend/.../dashboard/components/widget-container/widget-container.component.ts` | Modify | 5 |
| `src/frontend/.../dashboard/graphql/dashboard.operations.ts` | Modify | 5 |
| `src/frontend/.../dashboard/services/dashboard.service.ts` | Modify | 5 |
| `src/frontend/.../dashboard/widgets/welcome-widget/welcome-widget.component.ts` | Modify | 6 |
| `src/frontend/.../dashboard/widgets/upcoming-events-widget/upcoming-events-widget.component.ts` | Modify | 7 |

## Verification

1. **Backend**: Run `dotnet test` -- widget provider tests verify ConfigSchema is set
2. **Frontend**: Run `ng serve`, navigate to dashboard, enter edit mode
   - Click gear icon on Welcome Widget -> change greeting -> Apply -> verify greeting updates
   - Click gear icon on Upcoming Events -> change days/max entries -> Apply -> verify list updates
   - Refresh page -> verify config persisted (greeting and event settings retained)
3. **GraphQL**: Test `updateWidgetConfig` mutation via Banana Cake Pop / playground
