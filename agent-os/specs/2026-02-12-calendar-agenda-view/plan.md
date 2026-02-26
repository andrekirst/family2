# Calendar Agenda View — Plan

## Context

**User story:** As a family member, I want to see an agenda view in the shared family calendar so that I can quickly scan upcoming events in a simple, chronological list without the spatial overhead of a grid.

**Problem:** Month, week, and day views are spatial grids optimized for time-slot visualization. Families often just want a "what's coming up" list — a quick scan of upcoming events across days without needing to interpret a grid layout.

**Outcome:** A scrollable chronological event list grouped by day, accessible as the fourth view mode (Month | Week | Day | Agenda). Events are grouped under sticky day headers with "Today" / "Tomorrow" labels, all-day events shown at top of each group, and a "Load more" button for incremental 30-day batches.

**Scope:** Frontend-only. The existing `GetCalendarEventsQuery` with date-range filtering is sufficient. No backend changes required.

**Reference model:** Google Calendar's agenda view / Apple Calendar list view.

---

## Task 1: Save Spec Documentation

Create `agent-os/specs/2026-02-12-calendar-agenda-view/` with:

- **plan.md** — This full plan
- **shape.md** — Shaping notes (scope, decisions, context)
- **standards.md** — `frontend/angular-components` + `testing/unit-testing`
- **references.md** — Pointers to existing calendar code

---

## Task 2: Update Models and Types

**File:** `src/frontend/family-hub-web/src/app/features/calendar/models/calendar.models.ts`

- Extend `CalendarViewMode`: `'month' | 'week' | 'day' | 'agenda'`
- Add `AgendaDayGroup` interface (date, label, isToday, allDayEvents, timedEvents)
- Add `AGENDA_CONSTANTS` (BATCH_DAYS: 30, MAX_BATCHES: 12)

---

## Task 3: Create Agenda Utility Functions

**New file:** `src/frontend/family-hub-web/src/app/features/calendar/utils/agenda.utils.ts`

- `getAgendaDateRange(batchCount)` — Computes start (today) and end (today + batchCount * 30 days)
- `groupEventsByDay(events, batchCount)` — Groups events into day buckets, skips empty days, partitions all-day vs. timed, excludes cancelled events, sorts timed by start time
- `formatAgendaDayHeader(date)` — "Today — Thursday, Feb 12" / "Tomorrow — Friday, Feb 13" / plain date
- `formatAgendaEventTime(event, day)` — Time range with "(continues)" / "(continued)" for multi-day events

---

## Task 4: Create Agenda Utility Tests

**New file:** `src/frontend/family-hub-web/src/app/features/calendar/utils/agenda.utils.spec.ts`

Following `week.utils.spec.ts` patterns:

- `getAgendaDateRange` — start of today, correct end for 1/2 batches
- `groupEventsByDay` — empty, grouping, skip empty days, cancelled exclusion, all-day/timed separation, sort order, today marking, multi-day inclusion, out-of-range exclusion
- `formatAgendaDayHeader` — Today prefix, Tomorrow prefix, plain date, locale support
- `formatAgendaEventTime` — single-day range, continues, continued, all-day continuation, 24h format

---

## Task 5: Create CalendarAgendaComponent

**New file:** `src/frontend/family-hub-web/src/app/features/calendar/components/calendar-agenda/calendar-agenda.component.ts`

- Standalone component with signal-backed inputs (events, batchCount, loading, loadingMore, hasMore)
- Computed `dayGroups` from `groupEventsByDay()` utility
- Sticky day headers with blue tint for today
- All-day events: colored dot + title + "All day" pill + optional location
- Timed events: left border accent + time column + title/description/location + attendee count
- "Load more" button with spinner state
- Empty state with calendar icon when no events
- Scroll-to-today on initial load via `ngAfterViewInit`
- `data-testid` attributes on key elements

---

## Task 6: Update CalendarViewSwitcherComponent

**File:** `src/frontend/family-hub-web/src/app/features/calendar/components/calendar-view-switcher/calendar-view-switcher.component.ts`

Add fourth "Agenda" button with identical toggle styling pattern. Uses `data-testid="view-agenda-btn"`.

---

## Task 7: Integrate into CalendarPageComponent

**File:** `src/frontend/family-hub-web/src/app/features/calendar/components/calendar-page/calendar-page.component.ts`

- Add agenda-specific state signals: `agendaEvents`, `agendaBatchCount`, `agendaLoadingMore`, `agendaHasMore`
- Navigation label: "Upcoming Events" for agenda mode
- View switching logic: agenda ↔ month/week/day transitions (agenda uses today as anchor)
- Hide prev/next arrows in agenda mode
- `goToToday()` resets agenda batch count
- `loadAgendaEvents(isLoadMore)` — fetches events for rolling date range, distinguishes initial load vs. load-more spinner
- `onLoadMoreAgenda()` — increments batch count, caps at MAX_BATCHES
- `reloadCurrentView()` — routes to correct load method after event CRUD
- Template section with loading skeleton and agenda component bindings

---

## Files Modified (3)

| File | Change |
|------|--------|
| `models/calendar.models.ts` | Extend `CalendarViewMode`, add `AgendaDayGroup`, add `AGENDA_CONSTANTS` |
| `components/calendar-view-switcher/calendar-view-switcher.component.ts` | Add "Agenda" toggle button |
| `components/calendar-page/calendar-page.component.ts` | Agenda state, view switching, loading, template |

All paths relative to `src/frontend/family-hub-web/src/app/features/calendar/`.

## Files Created (3)

| File | Purpose |
|------|---------|
| `utils/agenda.utils.ts` | Day grouping, header formatting, time formatting, date range |
| `utils/agenda.utils.spec.ts` | 22 tests for all utility functions |
| `components/calendar-agenda/calendar-agenda.component.ts` | Standalone agenda view component |

---

## Out of Scope

- Backend changes (none needed — existing `GetCalendarEventsQuery` suffices)
- Infinite scroll (using explicit "Load more" button instead)
- Event creation from agenda view (use existing "+ New Event" top bar action)
- Touch/swipe gestures for mobile
- Agenda view as default/remembered view preference

---

## Verification

1. **Build:** `cd src/frontend/family-hub-web && npm run build` — no compilation errors
2. **Unit tests:** `npm test -- --watch=false` — 334 tests pass (22 new agenda tests)
3. **Manual testing (agenda view):**
   - View switcher shows Month | Week | Day | Agenda
   - Events grouped by day with sticky headers
   - Today's group highlighted with blue background
   - All-day events at top with colored dot
   - "Load more" loads next 30-day batch
   - Click event → context panel opens for editing
   - Empty state when no events
   - Today button resets to first batch
   - Prev/next arrows hidden
   - Seamless transitions to/from other views
