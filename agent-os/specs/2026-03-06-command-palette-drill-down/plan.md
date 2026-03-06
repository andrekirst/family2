# Command Palette Hierarchical Sub-Item Navigation (Spotlight Drill-Down)

**Created**: 2026-03-06
**GitHub Issue**: #220
**Spec**: `agent-os/specs/2026-03-06-command-palette-drill-down/`

## Context

The command palette (`Ctrl+K`) already exists with NLP parsing, GraphQL search, keyboard navigation, and grouped results. However, all items are displayed as a **flat list** grouped by type. This feature adds **macOS Spotlight-style drill-down navigation**: selecting a group replaces the entire view with its sub-items, with a back button and breadcrumb trail. Supports **unlimited nesting depth**.

**User Story**: As a family member, I want to drill into command palette groups to see sub-items, so that I can quickly find and execute specific actions without leaving the keyboard.

## Files to Modify

| Action | File | Purpose |
|--------|------|---------|
| **Modify** | `src/frontend/family-hub-web/src/app/shared/models/search.models.ts` | Add `children`, `isGroup`, `id` to `PaletteItem`; add `PaletteNavigationFrame` |
| **Modify** | `src/frontend/family-hub-web/src/app/shared/services/command-palette.service.ts` | Navigation stack, `drillInto()`, `goBack()`, scoped search, breadcrumbs |
| **Create** | `src/frontend/family-hub-web/src/app/shared/services/palette-registry.service.ts` | Hierarchical item registry (extracts + extends `getDefaultItems()`) |
| **Modify** | `src/frontend/family-hub-web/src/app/shared/components/command-palette/command-palette.component.ts` | Back header, breadcrumbs, chevron indicators, slide animation, keyboard updates |

## Implementation Tasks

### Task 1: Save Spec, Commit, and Create GitHub Issue

1. Write spec files to `agent-os/specs/2026-03-06-command-palette-drill-down/`
2. Create GitHub issue with labels: `type-feature`, `status-planning`, `phase-1`, `service-frontend`, `priority-p2`, `effort-m`
3. Update spec files with issue number
4. Git commit: `docs(spec): add command-palette-drill-down spec (#N)`

### Task 2: Extend Data Model (`search.models.ts`)

Add to `PaletteItem`:

```typescript
children?: PaletteItem[];    // Sub-items (presence = drillable group)
isGroup?: boolean;           // Explicit group flag (chevron indicator)
id?: string;                 // Stable identity for @for tracking
```

Add new interface:

```typescript
export interface PaletteNavigationFrame {
  label: string;             // Breadcrumb label
  items: PaletteItem[];      // Unfiltered items at this level
  query: string;             // Search query active when we drilled in
}
```

Backward compatible -- existing flat items just lack `children`.

### Task 3: Create Palette Registry Service (`palette-registry.service.ts`)

Extract `getDefaultItems()` from `CommandPaletteService` into a dedicated `PaletteRegistryService`:

- `getRootItems(): PaletteItem[]` -- returns all registered root items
- `registerItems(items: PaletteItem[])` -- allows feature modules to add groups
- Built-in groups: Calendar (Create Event, Today, Week View, Month View), Family (Invite Member, Members, Settings), Dashboard, Messages, Files
- Hint items remain in `CommandPaletteService` (daily-rotating, not registry items)

### Task 4: Add Navigation Stack to Service (`command-palette.service.ts`)

New signals:

- `navigationStack = signal<PaletteNavigationFrame[]>([])`
- `isDrilledIn = computed(() => this.navigationStack().length > 0)`
- `breadcrumbs = computed(() => this.navigationStack().map(f => f.label))`
- `slideDirection = signal<'left' | 'right' | 'none'>('none')`

New methods:

- `drillInto(item)` -- push current state onto stack, show item's children, animate left
- `goBack()` -- pop stack, restore previous state, animate right
- `goToRoot()` -- clear entire stack

Modify existing:

- `executeItem()` -- check `item.children?.length || item.isGroup` before routing; if true, `drillInto()` instead
- `performSearch()` -- when `isDrilledIn()`, filter current children client-side instead of GraphQL call
- `open()` / `close()` -- reset `navigationStack` to `[]`
- Wire up `PaletteRegistryService` for `getDefaultItems()`

### Task 5: Update Component Template & Keyboard Handling (`command-palette.component.ts`)

**Template additions:**

1. **Breadcrumb/back header** (when `isDrilledIn()`): back button + breadcrumb trail with `/` separators
2. **Chevron indicator** on items with `children`: right-arrow SVG after module badge
3. **Scoped search placeholder**: "Search in {groupLabel}..." when drilled in
4. **Slide animation wrapper**: CSS-only transition using `slideDirection` signal (opacity + translateX, 150ms)
5. **Footer hints**: Add ArrowRight "drill in" and ArrowLeft "back" keyboard hints

**Keyboard additions:**

- `ArrowRight` on group item -> `drillInto()`
- `ArrowLeft` when drilled in -> `goBack()`
- `Backspace` when drilled in AND query is empty -> `goBack()`
- `Enter` on group item -> `drillInto()` (instead of navigate)

**Animation approach** (CSS-only, no `@angular/animations`):

- `slidePhase` signal: `'idle' | 'exit' | 'enter'`
- On drill/back: set `exit` -> wait frame -> update items -> set `enter` -> wait 150ms -> set `idle`
- Tailwind classes: `opacity-0 -translate-x-2` (exit-left), `opacity-0 translate-x-2` (exit-right), `opacity-100 translate-x-0 transition-all duration-150` (idle)

### Task 6: Verify

- `ng build` succeeds
- Open palette with `Ctrl+K` -> see groups with chevron indicators
- Arrow keys navigate, `ArrowRight`/`Enter` on group drills in
- Breadcrumb shows path, back button works
- `ArrowLeft`/`Backspace` (empty query) goes back
- Scoped search filters within current group
- Slide animation plays on drill-in and back
- All existing features still work (NLP, search, hints, Esc to close)
- Deeply nested items (3+ levels) work correctly
