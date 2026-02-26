# References for Calendar Agenda View

## Existing Calendar Infrastructure

### CalendarPageComponent (orchestrator — modified)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/components/calendar-page/calendar-page.component.ts`
- **Relevance:** Manages view mode, event loading, and context panel orchestration. Extended with agenda-specific state signals (`agendaEvents`, `agendaBatchCount`, `agendaLoadingMore`, `agendaHasMore`) and a separate `loadAgendaEvents()` method.
- **Key patterns:**
  - Signal-based state: `viewMode`, `events`, `isLoading`, `selectedDate`, `contextEvent`
  - View switching in `onViewModeChanged()` handles transitions between all four modes
  - `ng-template #eventContextTemplate` for context panel content
  - `ContextPanelService.open(template)` to show the side panel

### CalendarViewSwitcherComponent (modified)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/components/calendar-view-switcher/calendar-view-switcher.component.ts`
- **Relevance:** Toggle buttons for view mode selection. Extended with fourth "Agenda" button following identical styling pattern.
- **Key patterns:**
  - `@Input() activeView` + `@Output() viewChanged`
  - Conditional Tailwind classes for active/inactive state

### CalendarDayGridComponent (pattern reference)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/components/calendar-day-grid/calendar-day-grid.component.ts`
- **Relevance:** Used as the primary pattern reference for the agenda component structure. Same signal-backed input pattern, AfterViewInit scrolling, event click handling.
- **Key patterns:**
  - `@Input() set eventsInput(value)` → `events = signal()`
  - `@Output() eventClicked`
  - `ngAfterViewInit()` for auto-scroll behavior

### Week Utilities (reused functions)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/utils/week.utils.ts`
- **Key functions:**
  - `getStoredLocale()` — reads locale from localStorage
  - `formatTimeShort(date)` — formats time with 12h/24h preference
  - `getEventsForDay(events, day)` — filters events overlapping a given day
  - `partitionEvents(events, day)` — separates all-day vs. timed events

### Day Utilities (reused functions)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/utils/day.utils.ts`
- **Key functions:**
  - `getDayStart(date)` — returns date at 00:00:00.000
  - `getDayEnd(date)` — returns date at 23:59:59.999
  - `isToday(date)` — compares year/month/date

### Calendar Models (modified)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/models/calendar.models.ts`
- **Changes:**
  - Extended `CalendarViewMode` type with `'agenda'`
  - Added `AgendaDayGroup` interface
  - Added `AGENDA_CONSTANTS` (BATCH_DAYS: 30, MAX_BATCHES: 12)

### Week Utils Tests (pattern reference)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/utils/week.utils.spec.ts`
- **Relevance:** Template for test structure. 52 tests with `makeEvent()` helper, `vi.useFakeTimers()`, localStorage setup/teardown.

### CalendarService (reused, not modified)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/services/calendar.service.ts`
- **Relevance:** `getCalendarEvents(familyId, startDate, endDate)` is called by `loadAgendaEvents()` with the rolling date range.

## Backend (no changes needed)

### GetCalendarEventsQuery

- **Location:** `src/FamilyHub.Api/Features/Calendar/Application/Queries/GetCalendarEventsQuery.cs`
- **Relevance:** Already supports date-range filtering. The agenda view's rolling 30-day windows use the same query as month/week/day views.

### GraphQL Query

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/graphql/calendar.operations.ts`
- **Relevance:** `GetCalendarEvents` query already fetches all needed fields (id, title, description, location, startTime, endTime, isAllDay, isCancelled, attendees).
