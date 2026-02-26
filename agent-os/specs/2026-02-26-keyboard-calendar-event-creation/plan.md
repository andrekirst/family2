# Keyboard-First Calendar Event Creation — Implementation Plan

**Created**: 2026-02-26
**Spec Folder**: `agent-os/specs/2026-02-26-keyboard-calendar-event-creation/`

---

## Context

The calendar's `EventContextComponent` sidebar and its `DateTimePickerComponent` require all date/time input via mouse clicks. Time adjustment uses +/- buttons in 15-minute increments with no direct text entry. The calendar grid lacks keyboard navigation (arrow keys, Tab, Enter/Space). This creates an accessibility barrier and slows power users who prefer keyboard-driven workflows.

This plan covers **spec documentation and GitHub issue creation only**. Implementation tasks are documented here for future reference but are deferred.

---

## Decisions Made (Interview Summary)

| Topic | Decision |
|-------|----------|
| **Time input method** | Direct text input (`<input type="time">` or masked HH:MM field) alongside existing +/- buttons |
| **Calendar grid navigation** | Arrow keys for day-to-day movement, Enter/Space to select, Tab to enter/exit grid |
| **Focus management** | Auto-focus title field on open; logical Tab order through all form fields |
| **Time precision** | Allow minute-level precision via keyboard; keep 15-min snap for +/- buttons only |
| **Accessibility standard** | WCAG 2.1 AA compliance (keyboard operable, focus visible, ARIA labels) |
| **Backward compatibility** | Mouse interactions unchanged; keyboard is additive |
| **Component scope** | Modify `DateTimePickerComponent` and `EventContextComponent` only |
| **Testing approach** | Unit tests for keyboard handlers + Playwright E2E for full keyboard flow |
| **i18n** | Time format respects locale (12h/24h); ARIA labels use i18n strings |
| **Screen reader support** | ARIA roles (`grid`, `gridcell`), `aria-label` on calendar days, live regions for time changes |

---

## Future Implementation Tasks

### Task 1: Add Direct Time Input to DateTimePickerComponent

**Goal**: Replace read-only `<span>` time display with editable `<input>` fields.

**Changes**:

- Replace `<span>{{ formatTimeOnly(editStartTime()) }}</span>` with `<input type="time">` (or masked text input)
- Bind input value to `editStartTime` / `editEndTime` signals
- Parse keyboard-entered time on blur/Enter, validate format
- Keep +/- buttons alongside for mouse users
- Emit `dateTimeChanged` on valid keyboard input
- Snap keyboard input to nearest minute (not 15-min) for precision

**Files to modify**:

- `src/frontend/family-hub-web/src/app/shared/components/date-time-picker/date-time-picker.component.ts`

**Verification**:

- [ ] Can type `14:30` directly into start time field
- [ ] Tab key moves from start time to end time
- [ ] Invalid time input shows validation feedback
- [ ] +/- buttons still work as before

### Task 2: Add Keyboard Navigation to Calendar Grid

**Goal**: Make the calendar date grid navigable via keyboard (arrow keys, Enter, Escape).

**Changes**:

- Add `tabindex="0"` to focused calendar day, `tabindex="-1"` to others (roving tabindex)
- Handle `ArrowUp/Down/Left/Right` for day navigation
- `Enter` or `Space` to select a day
- `Escape` to return focus to month navigation
- `Home`/`End` for first/last day of week
- `PageUp`/`PageDown` for previous/next month
- Track focused day in a signal separate from selected day

**Files to modify**:

- `src/frontend/family-hub-web/src/app/shared/components/date-time-picker/date-time-picker.component.ts`

**Verification**:

- [ ] Arrow keys move focus through calendar days
- [ ] Enter selects the focused day
- [ ] PageUp/PageDown navigates months
- [ ] Focus is visually indicated (ring/outline)
- [ ] Mouse click still works as before

### Task 3: Add ARIA Attributes and Screen Reader Support

**Goal**: Make the date-time picker screen-reader accessible.

**Changes**:

- Add `role="grid"` to calendar container, `role="row"` to week rows, `role="gridcell"` to days
- Add `aria-label` to each day button (e.g., "Monday, 26 February 2026")
- Add `aria-selected="true"` on selected day
- Add `aria-current="date"` on today
- Add `aria-live="polite"` region to announce time changes
- Add `aria-label` to +/- buttons ("Decrease start time by 15 minutes")
- Add `aria-label` to month navigation buttons

**Files to modify**:

- `src/frontend/family-hub-web/src/app/shared/components/date-time-picker/date-time-picker.component.ts`

**Verification**:

- [ ] Screen reader announces selected date in full format
- [ ] Time changes are announced via live region
- [ ] All interactive elements have accessible names

### Task 4: Improve Tab Order in EventContextComponent

**Goal**: Ensure logical keyboard flow through the entire event creation sidebar.

**Changes**:

- Audit and fix Tab order: Title → Date Picker → Start Time → End Time → All-Day → Description → Attendees → Save
- Ensure `InlineEditTextComponent` enters edit mode on Enter/Space (not just click)
- Add keyboard shortcut for Save (Ctrl+Enter)
- Add Escape to close the context panel

**Files to modify**:

- `src/frontend/family-hub-web/src/app/features/calendar/components/event-context/event-context.component.ts`
- `src/frontend/family-hub-web/src/app/shared/components/inline-edit-text/` (if needed)

**Verification**:

- [ ] Tab moves through all form fields in logical order
- [ ] Ctrl+Enter saves the event
- [ ] Escape closes the sidebar
- [ ] No focus traps

### Task 5: Unit Tests for Keyboard Interactions

**Goal**: Cover all keyboard handlers with unit tests.

**Changes**:

- Test arrow key navigation in calendar grid (boundary cases: month wrap, week wrap)
- Test direct time input parsing (valid/invalid formats, edge cases)
- Test Tab order assertions
- Test Escape/Enter key behaviors

**Files to create**:

- Update `src/frontend/family-hub-web/src/app/shared/components/date-time-picker/date-time-picker.component.spec.ts`

**Verification**:

- [ ] Arrow key navigation tests pass
- [ ] Time input parsing tests pass
- [ ] Focus management tests pass

### Task 6: Playwright E2E Tests

**Goal**: End-to-end test for complete keyboard-only event creation flow.

**Changes**:

- Test: Tab to calendar → arrow to date → Enter → Tab to time → type time → Tab → type title → Ctrl+Enter
- Test: Full event creation without any mouse interaction
- Test: Screen reader announcements (if Playwright supports ARIA assertions)

**Files to create**:

- `src/frontend/family-hub-web/e2e/calendar-keyboard.spec.ts`

**Verification**:

- [ ] Can create a full event using only keyboard
- [ ] All focus states visible during flow

---

## Existing Files/Patterns to Reuse

- `src/frontend/family-hub-web/src/app/shared/components/date-time-picker/date-time-picker.component.ts` — Current picker (modify)
- `src/frontend/family-hub-web/src/app/features/calendar/components/event-context/event-context.component.ts` — Event sidebar (modify)
- `src/frontend/family-hub-web/src/app/shared/components/inline-edit-text/` — Inline edit (may need keyboard audit)
- `src/frontend/family-hub-web/src/app/shared/components/confirmation-dialog/` — Dialog (keyboard trap pattern)
- Angular CDK `FocusKeyManager` / `FocusTrapFactory` — Potential utility for focus management

---

## Verification (Spec Task)

After execution of the spec-only tasks:

1. **Spec folder exists** at `agent-os/specs/2026-02-26-keyboard-calendar-event-creation/` with 4 files
2. **GitHub issue created** and visible at the returned URL
3. **Labels applied** correctly to the issue
4. **Spec files** are internally consistent and reference correct existing file paths
