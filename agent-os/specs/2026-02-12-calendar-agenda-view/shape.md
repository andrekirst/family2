# Calendar Agenda View — Shaping Notes

## Scope

As a family member, I want to see an agenda view in the shared family calendar so that I can quickly scan upcoming events in a simple, chronological list without the spatial overhead of a grid.

**In scope:**

- Scrollable event list grouped by day as fourth view mode
- Day headers with "Today" / "Tomorrow" contextual labels
- All-day events shown at top of each day group
- Timed events sorted chronologically within each group
- Multi-day event annotations: "(continues)" / "(continued)"
- 30-day rolling window with "Load more" button (up to 12 batches = ~1 year)
- Cancelled events hidden (consistent with month/week/day views)
- Event click opens existing edit context panel
- Empty state when no upcoming events
- Today button scrolls/resets to today
- Prev/next arrows hidden in agenda mode

**Out of scope:**

- Backend changes (existing `GetCalendarEventsQuery` with date-range filtering suffices)
- Infinite scroll (explicit "Load more" button for simplicity)
- Event creation from within the agenda list
- Touch/swipe gestures for mobile
- Agenda as default/remembered view preference
- Recurring event expansion

## Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| View format | Scrollable event list grouped by day | Simplest "what's coming up" mental model |
| All-day events | Shown at top of each day group with colored dot | Visual distinction from timed events |
| Date range | 30-day rolling window from today | Balances initial load speed with useful range |
| Load more | Explicit button, not infinite scroll | Predictable, accessible, no scroll jank |
| Max range | 12 batches × 30 days = ~1 year | Prevents unbounded queries |
| Cancelled events | Hidden | Consistent with other view modes |
| Event interaction | Reuse existing `EventContextComponent` | No new dialogs needed |
| Navigation | Today button + scroll only, no prev/next arrows | Agenda is a forward-looking list, not period-based |
| Empty days | Skipped (no empty day headers) | Reduces noise in sparse calendars |
| View transitions | Agenda → other modes uses "today" as anchor | Agenda doesn't track a specific date |
| Separate events signal | `agendaEvents` separate from `events` | Different loading model (rolling window vs. fixed period) |

## Context

- **Visuals:** Google Calendar agenda view / Apple Calendar list view
- **References:** Existing CalendarPageComponent, CalendarViewSwitcherComponent, week.utils.ts, day.utils.ts
- **Product alignment:** Calendar is Phase 1 Critical Path. Agenda view completes the standard set of calendar view modes.
- **Issue:** #131

## Standards Applied

- **frontend/angular-components** — Signals for state, standalone components, `@Input() set` pattern for signal bridging
- **testing/unit-testing** — Vitest with `vi.useFakeTimers()`, `vi.setSystemTime()` for deterministic date testing
