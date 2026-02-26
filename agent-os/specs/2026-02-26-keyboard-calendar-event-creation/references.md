# References for Keyboard-First Calendar Event Creation

This document captures the code references studied during the exploration phase to inform the keyboard accessibility implementation.

---

## Primary Components to Modify

### 1. DateTimePickerComponent

**Location**: `src/frontend/family-hub-web/src/app/shared/components/date-time-picker/date-time-picker.component.ts`

**Current state**: Inline template with mouse-only interactions.

**Time display (read-only span, lines 133-138)**:

```html
<span
  class="text-sm font-mono w-16 text-center"
  [attr.data-testid]="testId + '-start-time'"
>
  {{ formatTimeOnly(editStartTime()) }}
</span>
```

**Time adjustment (button-only, lines 124-148)**:

```html
<button (click)="adjustTime('start', -15)"> &minus; </button>
<span>{{ formatTimeOnly(editStartTime()) }}</span>
<button (click)="adjustTime('start', 15)"> + </button>
```

**Calendar grid (click-only, lines 83-99)**:

```html
<div class="grid grid-cols-7 gap-0">
  @for (day of calendarDays(); track day.date.getTime()) {
    <button type="button" (click)="selectDate(day.date)" ...>
      {{ day.dayNumber }}
    </button>
  }
</div>
```

**Relevance**: This is the primary file to modify. The time `<span>` becomes an `<input>`, and the calendar grid gains keyboard handlers and ARIA attributes.

**Key methods to extend**:

- `adjustTime(which, minutes)` — Keep as-is for buttons; add new `onTimeInput(which, value)` for keyboard entry
- `selectDate(date)` — Keep as-is for click; add keyboard selection path
- New: `onCalendarKeydown(event, day)` — Arrow key handler for grid navigation
- New: `focusedDay` signal — Track which day has keyboard focus (separate from selection)

### 2. EventContextComponent

**Location**: `src/frontend/family-hub-web/src/app/features/calendar/components/event-context/event-context.component.ts`

**Current state**: Sidebar for event creation/editing. Uses `DateTimePickerComponent`, `InlineEditTextComponent`, `ConfirmationDialogComponent`.

**Relevance**: Tab order audit. Add `Ctrl+Enter` save shortcut and `Escape` close shortcut. Ensure focus flows logically through the form.

**Key outputs to preserve**:

```typescript
@Output() eventCreated = new EventEmitter<CalendarEventDto>();
@Output() eventUpdated = new EventEmitter<void>();
@Output() eventCancelled = new EventEmitter<void>();
```

### 3. InlineEditTextComponent

**Location**: `src/frontend/family-hub-web/src/app/shared/components/inline-edit-text/`

**Relevance**: Used for title and description in EventContextComponent. May need keyboard audit — ensure Enter/Space triggers edit mode (not just click).

---

## Supporting Components (Read-Only Reference)

### 4. ContextPanelService

**Location**: `src/frontend/family-hub-web/src/app/shared/services/context-panel.service.ts`

**Relevance**: Controls sidebar open/close state. The `Escape` key handler in EventContextComponent will call this service to close the panel.

### 5. CalendarService

**Location**: `src/frontend/family-hub-web/src/app/features/calendar/services/calendar.service.ts`

**Relevance**: Not modified. Event creation/update GraphQL operations remain unchanged. The keyboard improvements only change how users fill in the form, not what happens on submit.

### 6. ConfirmationDialogComponent

**Location**: `src/frontend/family-hub-web/src/app/shared/components/confirmation-dialog/`

**Relevance**: Already uses a dialog pattern. Good reference for focus trapping (focus should stay within dialog when open). The keyboard accessibility pattern here can inform the calendar grid's focus management.

---

## Existing Test Infrastructure

### 7. DateTimePickerComponent Spec

**Location**: `src/frontend/family-hub-web/src/app/shared/components/date-time-picker/date-time-picker.component.spec.ts`

**Relevance**: Existing test file to extend with keyboard interaction tests.

### 8. E2E Test Directory

**Location**: `src/frontend/family-hub-web/e2e/`

**Relevance**: New Playwright test file `calendar-keyboard.spec.ts` will be created here.

---

## Patterns to Follow

### 9. Data-TestId Convention

**Existing pattern in DateTimePickerComponent**:

```html
[attr.data-testid]="testId + '-prev-month'"
[attr.data-testid]="testId + '-start-time'"
[attr.data-testid]="testId + '-start-minus'"
[attr.data-testid]="testId + '-allday'"
[attr.data-testid]="testId + '-calendar-grid'"
```

**New testids to add**:

- `testId + '-start-time-input'` — Direct time input field (start)
- `testId + '-end-time-input'` — Direct time input field (end)
- `testId + '-day-{N}'` — Individual calendar day (for E2E targeting)

### 10. Signal-Based State

**Existing pattern**:

```typescript
readonly viewMonth = signal(new Date());
readonly editStartTime = signal('');
readonly editEndTime = signal('');
readonly editAllDay = signal(false);
```

**New signal to add**:

```typescript
readonly focusedDate = signal<Date | null>(null);  // Keyboard-focused day (distinct from selected)
```

### 11. i18n Pattern

**Existing pattern**:

```typescript
readonly weekDays = [
  $localize`:@@calendar.dayMon:Mon`,
  $localize`:@@calendar.dayTue:Tue`,
  // ...
];
```

**New i18n strings needed**:

- `@@calendar.aria.prevMonth` — "Go to previous month"
- `@@calendar.aria.nextMonth` — "Go to next month"
- `@@calendar.aria.dayLabel` — "{{dayName}}, {{dayNumber}} {{month}} {{year}}"
- `@@calendar.aria.startTimeDecrease` — "Decrease start time by 15 minutes"
- `@@calendar.aria.startTimeIncrease` — "Increase start time by 15 minutes"
- `@@calendar.aria.endTimeDecrease` — "Decrease end time by 15 minutes"
- `@@calendar.aria.endTimeIncrease` — "Increase end time by 15 minutes"

---

## External References

### WAI-ARIA Date Picker Dialog

**URL**: https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/examples/datepicker-dialog/

**Relevance**: Canonical reference for keyboard interactions and ARIA attributes on calendar grids. The implementation should match this pattern closely.

### Angular CDK Focus Management

**Package**: `@angular/cdk/a11y`

**Relevant utilities**:

- `FocusKeyManager` — Manages keyboard focus in a list of items
- `FocusTrap` — Traps focus within a container (useful for dialogs, potentially for the sidebar)
- `LiveAnnouncer` — Announces changes to screen readers via `aria-live`

**Relevance**: Consider using `LiveAnnouncer` for time change announcements instead of manually managing `aria-live` regions.
