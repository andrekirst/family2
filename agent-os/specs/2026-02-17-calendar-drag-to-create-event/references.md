# References for Calendar Drag-to-Create Event

## Existing Calendar Infrastructure

### CalendarWeekGridComponent (primary modification target)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/components/calendar-week-grid/calendar-week-grid.component.ts`
- **Relevance:** This is where the drag interaction will be added. Already has the time grid, day columns, scroll container, and single-click handler.
- **Key patterns:**
  - Uses Angular signals for state (`weekStart`, `events`, `nowOffset`)
  - Inline template with Tailwind CSS
  - `@ViewChild('scrollContainer')` for scroll offset tracking
  - `onTimeSlotClick` converts pixel Y to Date via `pixelOffsetToTime()`
  - `@Output() timeSlotClicked` and `@Output() eventClicked` for parent communication

### EventContextComponent (accepts new inputs)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/components/event-context/event-context.component.ts`
- **Relevance:** The context panel that opens after drag. Already supports create mode (`selectedDate` input) and edit mode (`event` input). Will receive new `selectedStartDate`/`selectedEndDate` inputs.
- **Key patterns:**
  - Dirty tracking via `FormSnapshot` + `savedSnapshot` signal
  - `canSave` computed signal for save button state
  - `initializeForm()` branches on edit vs. create mode
  - Uses `CalendarService` for API calls, `ContextPanelService` for panel management

### CalendarPageComponent (orchestrator)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/components/calendar-page/calendar-page.component.ts`
- **Relevance:** Orchestrates the connection between the week grid and the context panel. Manages `selectedDate`, `contextEvent`, and view mode signals.
- **Key patterns:**
  - `ng-template #eventContextTemplate` for context panel content
  - `ContextPanelService.open(template)` to show the side panel
  - Mutual exclusivity between selectedDate/contextEvent

### Utility Functions (reused, not modified)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/utils/week.utils.ts`
- **Key functions:**
  - `pixelOffsetToTime(yOffset, dayDate)` — converts pixel Y to Date, snapped to 15-min
  - `timeToPixelOffset(date)` — converts Date to pixel Y
  - `formatTimeShort(date)` — formats "9:00 AM"

### Grid Constants (reused, not modified)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/models/calendar.models.ts`
- **Key constants:**
  - `HOUR_HEIGHT: 60` — 60px per hour (1px = 1 minute)
  - `SNAP_MINUTES: 15` — 15-minute snap intervals
  - `TOTAL_HEIGHT: 1440` — 24 hours * 60px
  - `MIN_EVENT_HEIGHT: 15` — minimum 15px

## Backend (no changes needed)

### CreateCalendarEventCommand

- **Location:** `src/FamilyHub.Api/Features/Calendar/Application/Commands/CreateCalendarEventCommand.cs`
- **Relevance:** Already handles event creation with all required fields (Title, Description, StartTime, EndTime, IsAllDay, AttendeeIds)

### GraphQL Mutation

- **Location:** `src/FamilyHub.Api/Features/Calendar/GraphQL/CalendarMutations.cs`
- **Relevance:** `Create(input: CreateCalendarEventRequest)` mutation is already wired up and working

### CalendarService (frontend)

- **Location:** `src/frontend/family-hub-web/src/app/features/calendar/services/calendar.service.ts`
- **Relevance:** `createCalendarEvent(input)` already calls the GraphQL mutation. Used by EventContextComponent.
