# Calendar Drag-to-Create Event — Plan

## Context

**User story:** As a family member, I want to create a calendar event by marking a time range in the calendar (Google Calendar-style drag-to-select on the week grid).

**Problem:** Currently, clicking a time slot in the week grid opens the context panel with a default 1-hour duration. There's no way to visually select a specific time range, which is the most intuitive interaction for calendar event creation.

**Outcome:** Users can mousedown on a day column in the week grid, drag vertically to select a time range (with a live blue overlay preview), and on mouseup the context panel opens with the exact start/end times pre-filled.

**Scope:** Frontend-only. The backend already has full CRUD support (CreateCalendarEventCommand, GraphQL mutation, etc.). The existing single-click behavior is preserved (< 15px movement = click).

**Reference model:** Google Calendar's drag-to-create + side panel.

---

## Task 1: Save Spec Documentation

Create `agent-os/specs/2026-02-17-calendar-drag-to-create-event/` with:

- **plan.md** — This full plan
- **shape.md** — Shaping notes (scope, decisions, context)
- **standards.md** — `frontend/angular-components` + `testing/unit-testing`
- **references.md** — Pointers to existing calendar code

---

## Task 2: Add `TimeRange` interface to models

**File:** `src/frontend/family-hub-web/src/app/features/calendar/models/calendar.models.ts`

Add:

```typescript
export interface TimeRange {
  start: Date;
  end: Date;
}
```

---

## Task 3: Add drag-to-select interaction to CalendarWeekGridComponent

**File:** `src/frontend/family-hub-web/src/app/features/calendar/components/calendar-week-grid/calendar-week-grid.component.ts`

This is the core of the feature. Changes:

### 3a. New drag state signals

```typescript
private isDragging = signal(false);
private dragDayIndex = signal<number | null>(null);
private dragStartY = signal(0);
private dragCurrentY = signal(0);
```

### 3b. Computed overlay for visual feedback

```typescript
dragOverlay = computed(() => {
  if (!this.isDragging()) return null;
  const top = Math.min(this.dragStartY(), this.dragCurrentY());
  const bottom = Math.max(this.dragStartY(), this.dragCurrentY());
  const snappedTop = Math.round(top / WEEK_GRID_CONSTANTS.SNAP_MINUTES) * WEEK_GRID_CONSTANTS.SNAP_MINUTES;
  const snappedBottom = Math.round(bottom / WEEK_GRID_CONSTANTS.SNAP_MINUTES) * WEEK_GRID_CONSTANTS.SNAP_MINUTES;
  return {
    top: Math.max(0, snappedTop),
    height: Math.max(WEEK_GRID_CONSTANTS.SNAP_MINUTES, snappedBottom - snappedTop),
    dayIndex: this.dragDayIndex()!,
  };
});
```

### 3c. New output

```typescript
@Output() timeRangeSelected = new EventEmitter<TimeRange>();
```

### 3d. Mouse event handlers

- **`onMouseDown(event, dayIndex)`** — Records start position, attaches document-level mousemove/mouseup listeners, prevents text selection
- **`onMouseMove`** (arrow function) — Updates `dragCurrentY`, clamped to grid bounds
- **`onMouseUp`** (arrow function) — If drag distance < 15px, treats as click (emits `timeSlotClicked`). If >= 15px, swaps start/end if needed and emits `timeRangeSelected` via `pixelOffsetToTime()`

### 3e. Template changes

- Replace `(click)="onTimeSlotClick($event, dayIdx)"` with `(mousedown)="onMouseDown($event, dayIdx)"` on day columns
- Add `[class.select-none]="isDragging()" [class.cursor-ns-resize]="isDragging()"` on scroll container
- Add drag overlay element inside each day column:

```html
@if (dragOverlay() && dragOverlay()!.dayIndex === dayIdx) {
  <div
    class="absolute left-0 right-0 bg-blue-500/20 border border-blue-400 rounded-sm z-30 pointer-events-none"
    [style.top.px]="dragOverlay()!.top"
    [style.height.px]="dragOverlay()!.height"
  >
    <div class="absolute top-0 left-1 text-[10px] font-medium text-blue-700">
      {{ formatDragTime(dragOverlay()!.top) }}
    </div>
    <div class="absolute bottom-0 left-1 text-[10px] font-medium text-blue-700">
      {{ formatDragTime(dragOverlay()!.top + dragOverlay()!.height) }}
    </div>
  </div>
}
```

### 3f. Cleanup

Update `ngOnDestroy` to remove document-level listeners.

### Reused utilities (no changes needed)

- `pixelOffsetToTime(yOffset, dayDate)` from `utils/week.utils.ts` — 15-min snap + clamping
- `WEEK_GRID_CONSTANTS` from `models/calendar.models.ts`

---

## Task 4: Add time-range inputs to EventContextComponent

**File:** `src/frontend/family-hub-web/src/app/features/calendar/components/event-context/event-context.component.ts`

Add two new `@Input()` properties:

```typescript
@Input() selectedStartDate: Date | null = null;
@Input() selectedEndDate: Date | null = null;
```

Add a new branch in `initializeForm()` between the `event` and `selectedDate` branches:

- If `selectedStartDate && selectedEndDate`, use them directly as start/end (no 1-hour default)
- Same reset logic as existing create mode (clear title/description, auto-focus title, etc.)

Update `ngOnChanges` to watch the new inputs.

---

## Task 5: Wire up CalendarPageComponent

**File:** `src/frontend/family-hub-web/src/app/features/calendar/components/calendar-page/calendar-page.component.ts`

Changes:

1. Add `selectedTimeRange = signal<TimeRange | null>(null)`
2. Add `onTimeRangeSelected(range: TimeRange)` handler — clears contextEvent/selectedDate, sets selectedTimeRange, opens context panel
3. Bind `(timeRangeSelected)="onTimeRangeSelected($event)"` on `<app-calendar-week-grid>`
4. Add template branch for time range:

   ```html
   @else if (selectedTimeRange()) {
     <app-event-context
       [selectedStartDate]="selectedTimeRange()!.start"
       [selectedEndDate]="selectedTimeRange()!.end"
       (eventCreated)="onEventCreated($event)"
     />
   }
   ```

5. Reset `selectedTimeRange` in `onTimeSlotClicked`, `onEventClicked`, `onDayClicked` for mutual exclusivity

---

## Task 6: Add unit tests

### CalendarWeekGridComponent spec (new file)

`src/frontend/family-hub-web/src/app/features/calendar/components/calendar-week-grid/calendar-week-grid.component.spec.ts`

Key test cases:

- Click behavior preserved (< 15px movement emits `timeSlotClicked`)
- Drag emits `timeRangeSelected` with correct start/end
- Bidirectional drag (upward) swaps start/end
- Overlay visible during drag, cleared after mouseup
- Event click passthrough (no drag on `.z-10` elements)

### EventContextComponent spec additions

- Create mode with time range sets correct start/end (no default duration)
- Title auto-focus triggered

---

## Files Modified

| File | Change |
|------|--------|
| `models/calendar.models.ts` | Add `TimeRange` interface |
| `components/calendar-week-grid/calendar-week-grid.component.ts` | Drag interaction, overlay, new output |
| `components/event-context/event-context.component.ts` | `selectedStartDate`/`selectedEndDate` inputs |
| `components/calendar-page/calendar-page.component.ts` | Wire up new event, template branch |
| `components/calendar-week-grid/calendar-week-grid.component.spec.ts` | New test file |

All paths relative to `src/frontend/family-hub-web/src/app/features/calendar/`.

---

## Out of Scope

- Touch/mobile support (touchstart/touchmove/touchend) — follow-up
- Month grid drag-to-create — follow-up
- Cross-day drag (stays within single column)
- Backend changes (none needed)

---

## Verification

1. **Build:** `cd src/frontend/family-hub-web && ng build` — no compilation errors
2. **Unit tests:** `ng test --watch=false` — all tests pass
3. **Manual testing (week view):**
   - Click a time slot -> context panel opens with 1-hour default (existing behavior preserved)
   - Drag a time range -> blue overlay appears during drag with time labels -> on release, context panel opens with exact start/end times
   - Drag upward -> start/end swapped correctly
   - Click on existing event -> opens edit mode (not intercepted by drag)
   - Create event via drag -> fill title -> save -> event appears on grid
4. **Lint:** `ng lint` — no new warnings
