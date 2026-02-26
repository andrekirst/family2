# Keyboard-First Calendar Event Creation — Shaping Notes

**Feature**: Keyboard accessibility and direct input for calendar event creation
**Created**: 2026-02-26

---

## User Story

**As a** power user or accessibility-dependent user,
**I want to** create and edit calendar events entirely via keyboard,
**So that** I don't need to rely on mouse clicks for date/time selection, improving both speed and accessibility.

---

## Scope

The calendar's `DateTimePickerComponent` and `EventContextComponent` currently require mouse interaction for all date and time input. This feature adds keyboard-first alternatives while preserving existing mouse behavior.

### What's In Scope (V1)

1. **Direct time text input**: Replace read-only time `<span>` with editable `<input>` fields
2. **Calendar grid keyboard navigation**: Arrow keys, Enter/Space, PageUp/PageDown
3. **ARIA attributes**: Screen reader support for the date-time picker
4. **Tab order audit**: Logical focus flow through EventContextComponent
5. **Keyboard shortcuts**: Ctrl+Enter to save, Escape to close panel
6. **Unit tests**: Keyboard interaction handlers
7. **E2E test**: Full keyboard-only event creation flow

### What's Out of Scope (Future)

1. Date text input (typing "2026-02-26" directly) — V1 uses calendar grid + keyboard nav
2. Drag-to-select date ranges — separate feature
3. Voice input / dictation support
4. Mobile-specific touch keyboard optimizations
5. Custom keyboard shortcut configuration
6. Calendar grid multi-select (shift+click)

---

## Decisions

### 1. Time Input Method

**Question**: How should users input time via keyboard?

**Answer**: **Native `<input type="time">` element** (or a masked HH:MM text field as fallback)

- Browsers provide built-in time input UX with keyboard support
- Preserves locale-aware formatting (12h/24h)
- +/- buttons remain alongside for mouse users who prefer incremental adjustment
- Keyboard input allows minute-level precision (not limited to 15-min intervals)

**Rationale**: `<input type="time">` gives us keyboard support, locale awareness, and validation for free. It's the most accessible option with the least custom code.

### 2. Calendar Grid Navigation Pattern

**Question**: What keyboard pattern should the calendar grid follow?

**Answer**: **WAI-ARIA Grid pattern with roving tabindex**

- One day in the grid has `tabindex="0"` (the focused day); all others have `tabindex="-1"`
- Arrow keys move focus between days; Tab moves focus out of the grid entirely
- This follows the established WAI-ARIA "Date Picker Dialog" pattern

**Rationale**: Roving tabindex is the standard pattern for grid navigation. It allows Tab to escape the grid (rather than tabbing through 42 days), while arrow keys provide intuitive spatial navigation.

### 3. Focus Management

**Question**: What should happen to focus when the event sidebar opens?

**Answer**: **Auto-focus title field** (already implemented), with logical Tab order through all fields

Tab order: Title → Calendar Grid → Start Time → End Time → All-Day Toggle → Description → Attendees → Save Button

**Rationale**: Title is the first thing users fill in. After that, the Tab order follows the visual layout top-to-bottom, matching user expectations.

### 4. Time Precision from Keyboard

**Question**: Should keyboard-entered times snap to 15-minute intervals?

**Answer**: **No — allow minute-level precision from keyboard**

- +/- buttons keep 15-minute snapping (existing behavior, useful for quick adjustment)
- Keyboard input accepts any valid minute (e.g., 14:07 is valid)
- This gives keyboard users *more* precision than mouse users

**Rationale**: The 15-minute snap is a UX convenience for button clicks. Keyboard users who type an exact time clearly want that exact time.

### 5. Backward Compatibility

**Question**: Do mouse-based interactions change?

**Answer**: **No changes to mouse behavior**

- +/- buttons remain and function identically
- Calendar day click selection unchanged
- All-day checkbox unchanged
- Keyboard support is purely additive

**Rationale**: Existing mouse users should notice no difference. Keyboard support is an enhancement, not a replacement.

### 6. ARIA and Screen Reader Support

**Question**: How much screen reader support should V1 include?

**Answer**: **WCAG 2.1 AA compliance**

- `role="grid"` / `role="gridcell"` on calendar
- `aria-label` with full date text on each day (e.g., "Wednesday, 26 February 2026")
- `aria-selected` and `aria-current="date"` attributes
- `aria-live="polite"` region for time change announcements
- Descriptive `aria-label` on all buttons

**Rationale**: AA is the standard compliance level for web applications. It covers keyboard operability, focus visibility, and programmatic semantics without requiring advanced assistive technology support (AAA).

### 7. Keyboard Shortcuts

**Question**: What global keyboard shortcuts should the event sidebar support?

**Answer**: **Minimal set**

- `Ctrl+Enter` — Save event (from anywhere in the form)
- `Escape` — Close sidebar (return to calendar)

**Rationale**: Only shortcuts that have clear, unambiguous meaning. Avoid conflicts with browser/OS shortcuts.

---

## Technical Constraints

1. **Angular standalone components**: All changes must use standalone component pattern
2. **Signals-based state**: Use Angular signals (not RxJS) for new state (consistent with existing code)
3. **i18n**: All new ARIA labels and accessible names must use `$localize` / `i18n` attributes
4. **No new dependencies**: Use Angular CDK if needed (already available), but no third-party date picker libraries
5. **Tailwind CSS**: Focus styles use Tailwind utilities (`focus:ring-2 focus:ring-blue-500`, etc.)
6. **Data-testid attributes**: All new interactive elements must have `data-testid` for E2E testing

---

## Success Indicators

### Functional

- [ ] Users can select a calendar date using only arrow keys + Enter
- [ ] Users can type a specific time (e.g., `14:30`) directly into the time field
- [ ] Users can Tab through the entire event form without mouse
- [ ] Users can save with Ctrl+Enter and close with Escape
- [ ] All existing mouse interactions still work

### Quality

- [ ] WCAG 2.1 AA compliance for DateTimePickerComponent
- [ ] All keyboard handlers unit tested
- [ ] Playwright E2E test for keyboard-only event creation
- [ ] Focus ring visible on all interactive elements during keyboard navigation
- [ ] No focus traps (Tab always escapes any component)

### User Experience

- [ ] Time entry via keyboard is faster than clicking +/- buttons repeatedly
- [ ] Calendar navigation feels natural (arrow = spatial movement)
- [ ] Screen readers announce dates in human-readable format
- [ ] Power users can create events without touching the mouse

---

## Risks & Mitigations

### Risk: `<input type="time">` inconsistent across browsers

**Mitigation**: Test on Chrome, Firefox, Safari. If browser support is insufficient, fall back to a masked text input with manual parsing. Angular CDK can help.

### Risk: Arrow key navigation conflicts with page scrolling

**Mitigation**: Only capture arrow keys when the calendar grid has focus (using `@HostListener` or `(keydown)` on the grid container). `event.preventDefault()` within the grid only.

### Risk: Focus management complexity with inline editing

**Mitigation**: The `InlineEditTextComponent` already handles click-to-edit. Ensure it also responds to Enter/Space to enter edit mode. Keep focus management within each component; don't try to manage cross-component focus from the parent.

### Risk: Tab order changes break existing E2E tests

**Mitigation**: Existing E2E tests use `data-testid` attributes (not Tab order). Adding `tabindex` attributes won't break existing selectors.

---

## Notes from Exploration

### Current DateTimePickerComponent (Mouse-Only)

- **Time display**: Read-only `<span class="text-sm font-mono">{{ formatTimeOnly(editStartTime()) }}</span>` — no input field
- **Time adjustment**: `adjustTime('start', ±15)` — button-click only, 15-min snap
- **Calendar grid**: Flat list of `<button>` elements with `(click)="selectDate(day.date)"` — no keyboard handlers
- **All-day toggle**: Standard `<input type="checkbox">` — already keyboard accessible
- **Month navigation**: `<button>` with `(click)` — keyboard accessible via Tab+Enter, but no PageUp/PageDown shortcut

### Current EventContextComponent

- **Title**: `InlineEditTextComponent` — click to edit, unclear keyboard support
- **Description**: `InlineEditTextComponent` — same concern
- **Attendees**: Multi-select from family members
- **Save button**: Standard `<button>` — keyboard accessible
- **No Ctrl+Enter shortcut** for save
- **No Escape shortcut** for close

### What Works Already

- All-day toggle (checkbox) is keyboard accessible
- Save/Cancel buttons are keyboard accessible (standard `<button>` elements)
- Month navigation arrows are keyboard accessible via Tab+Enter
