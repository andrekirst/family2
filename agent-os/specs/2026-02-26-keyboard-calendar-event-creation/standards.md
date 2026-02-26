# Standards for Keyboard-First Calendar Event Creation

The following standards apply to this work and guide the implementation decisions.

---

## 1. frontend/angular-components

**Source**: `agent-os/standards/frontend/angular-components.md`

### Standard

- All components must be `standalone: true` (no NgModules)
- Use Angular Signals for state management
- Follow atomic design hierarchy (Atoms → Molecules → Organisms → Templates → Pages)
- Import dependencies in `imports` array

### Application to This Feature

The `DateTimePickerComponent` is a **molecule** (combines input atoms with calendar grid). Changes stay within this component — no new components are created. All new state (e.g., `focusedDay` signal for roving tabindex) uses Angular signals.

**Key constraint**: The component uses an inline template. All keyboard event handlers and ARIA attributes are added directly to the inline template, keeping the single-file pattern.

---

## 2. frontend/apollo-graphql

**Source**: `agent-os/standards/frontend/apollo-graphql.md`

### Application to This Feature

**Not directly applicable.** This feature modifies UI interaction patterns only. No GraphQL operations are added or changed. The existing `CalendarService` GraphQL operations (create/update event) are unchanged.

---

## 3. testing/playwright-e2e

**Source**: `agent-os/standards/testing/playwright-e2e.md`

### Application to This Feature

A new Playwright E2E test validates the full keyboard-only event creation flow. The test:

- Uses `data-testid` selectors (existing convention in DateTimePickerComponent)
- Tests keyboard navigation: Tab, arrow keys, Enter, Escape, Ctrl+Enter
- Verifies focus visibility (`:focus-visible` pseudo-class)
- Validates that an event can be created without any mouse interaction

**Test location**: `src/frontend/family-hub-web/e2e/calendar-keyboard.spec.ts`

---

## 4. testing/unit-testing

**Source**: `agent-os/standards/testing/unit-testing.md`

### Standard

- xUnit/FluentAssertions/NSubstitute for backend (not applicable here)
- Arrange-Act-Assert pattern
- One assertion concept per test

### Application to This Feature

Frontend unit tests (Jasmine/Karma or Jest) for the `DateTimePickerComponent`:

- **Arrow key navigation**: Test that ArrowRight moves focus from day 15 to day 16, ArrowDown moves to day 22, etc.
- **Boundary navigation**: Test month-wrap behavior (ArrowRight on last day → first day of next month)
- **Time input parsing**: Test that typing `14:30` in the input emits correct `dateTimeChanged` event
- **Invalid time handling**: Test that invalid input (e.g., `25:00`, `abc`) shows validation error
- **Focus management**: Test that Tab escapes the calendar grid

---

## 5. Accessibility Standards (WCAG 2.1 AA)

**Source**: External standard — [W3C WCAG 2.1](https://www.w3.org/TR/WCAG21/)

### Applicable Success Criteria

| Criterion | Level | Relevance |
|-----------|-------|-----------|
| 2.1.1 Keyboard | A | All functionality operable via keyboard |
| 2.1.2 No Keyboard Trap | A | Focus must be movable away from any component |
| 2.4.3 Focus Order | A | Tab order matches visual/logical order |
| 2.4.7 Focus Visible | AA | Focus indicator visible during keyboard navigation |
| 4.1.2 Name, Role, Value | A | All UI components have accessible names and roles |

### Application to This Feature

- **2.1.1**: Add keyboard handlers to calendar grid and time inputs
- **2.1.2**: Ensure Tab escapes the calendar grid (roving tabindex, not sequential tabindex)
- **2.4.3**: Fix Tab order in EventContextComponent (Title → Calendar → Time → Description → Save)
- **2.4.7**: Add Tailwind focus utilities (`focus:ring-2 focus:ring-blue-500 focus:outline-none`)
- **4.1.2**: Add ARIA roles/labels to calendar grid and time adjustment buttons

---

## 6. WAI-ARIA Date Picker Pattern

**Source**: External pattern — [WAI-ARIA APG: Date Picker Dialog](https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/examples/datepicker-dialog/)

### Key Pattern Elements

| Element | ARIA Role | Keyboard |
|---------|-----------|----------|
| Calendar grid | `role="grid"` | Arrow keys navigate days |
| Week row | `role="row"` | — |
| Day cell | `role="gridcell"` | Enter/Space to select |
| Previous month button | `aria-label="Previous month"` | Focusable via Tab |
| Next month button | `aria-label="Next month"` | Focusable via Tab |

### Keyboard Interactions

| Key | Action |
|-----|--------|
| `ArrowRight` | Move to next day |
| `ArrowLeft` | Move to previous day |
| `ArrowDown` | Move to same day next week |
| `ArrowUp` | Move to same day previous week |
| `Home` | Move to first day of week |
| `End` | Move to last day of week |
| `PageUp` | Move to same day previous month |
| `PageDown` | Move to same day next month |
| `Enter` / `Space` | Select focused day |
| `Escape` | Close picker / return focus |

### Application to This Feature

The `DateTimePickerComponent` calendar grid will implement this exact pattern. The roving tabindex approach means only the focused day has `tabindex="0"`, allowing Tab to escape the grid to the next form field.

---

## How These Standards Apply Together

### Component Architecture (Standard #1)

The `DateTimePickerComponent` remains a standalone Angular component with signals. No structural changes — only template and behavior additions.

### Testing (Standards #3, #4)

Two layers: unit tests for individual keyboard handlers (Standard #4) and E2E tests for the full keyboard flow (Standard #3). Both use existing `data-testid` selectors.

### Accessibility (Standards #5, #6)

WCAG 2.1 AA provides the compliance target. The WAI-ARIA Date Picker pattern provides the specific implementation guide for the calendar grid. Together they define exactly what ARIA attributes and keyboard interactions are needed.

### No Backend Changes

Standards #2 (Apollo GraphQL) confirms that this feature is purely frontend. No GraphQL schema, query, or mutation changes. The `CalendarService.createCalendarEvent()` and `updateCalendarEvent()` calls remain identical — only the UI path to invoke them changes.
