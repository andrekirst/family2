import { CalendarEventDto } from '../services/calendar.service';
import { AgendaDayGroup, AGENDA_CONSTANTS } from '../models/calendar.models';
import { getStoredLocale, formatTimeShort } from './week.utils';
import { getDayStart, getDayEnd, isToday } from './day.utils';

// ── Date Range ──────────────────────────────────────────────────────

/**
 * Computes the agenda date range: from start of today to `batchCount * BATCH_DAYS` days out.
 */
export function getAgendaDateRange(batchCount: number): { start: Date; end: Date } {
  const start = getDayStart(new Date());
  const end = new Date(start);
  end.setDate(end.getDate() + batchCount * AGENDA_CONSTANTS.BATCH_DAYS);
  end.setHours(23, 59, 59, 999);
  return { start, end };
}

// ── Day Grouping ────────────────────────────────────────────────────

/**
 * Groups events into day buckets sorted chronologically.
 * Skips days with no events. Partitions all-day vs. timed within each group.
 * Cancelled events are excluded.
 */
export function groupEventsByDay(events: CalendarEventDto[], batchCount: number): AgendaDayGroup[] {
  const { start, end } = getAgendaDateRange(batchCount);
  const totalDays = Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24));
  const groups: AgendaDayGroup[] = [];

  for (let i = 0; i <= totalDays; i++) {
    const day = new Date(start);
    day.setDate(day.getDate() + i);

    const dayStart = getDayStart(day);
    const dayEnd = getDayEnd(day);

    const dayEvents = events.filter((e) => {
      if (e.isCancelled) return false;
      const eStart = new Date(e.startTime);
      const eEnd = new Date(e.endTime);
      return eStart <= dayEnd && eEnd >= dayStart;
    });

    if (dayEvents.length === 0) continue;

    const allDayEvents: CalendarEventDto[] = [];
    const timedEvents: CalendarEventDto[] = [];

    for (const e of dayEvents) {
      if (e.isAllDay) {
        allDayEvents.push(e);
      } else {
        timedEvents.push(e);
      }
    }

    // Sort timed events by start time
    timedEvents.sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime());

    groups.push({
      date: dayStart,
      label: formatAgendaDayHeader(day),
      isToday: isToday(day),
      allDayEvents,
      timedEvents,
    });
  }

  return groups;
}

// ── Formatting ──────────────────────────────────────────────────────

/**
 * Formats the day header for the agenda view.
 * - Today:    "Today — Thursday, Feb 12"
 * - Tomorrow: "Tomorrow — Friday, Feb 13"
 * - Other:    "Saturday, Feb 14"
 */
export function formatAgendaDayHeader(date: Date): string {
  const locale = getStoredLocale();
  const formatted = date.toLocaleDateString(locale, {
    weekday: 'long',
    month: 'short',
    day: 'numeric',
  });

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const target = new Date(date);
  target.setHours(0, 0, 0, 0);

  const diffDays = Math.round((target.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));

  if (diffDays === 0) return `Today — ${formatted}`;
  if (diffDays === 1) return `Tomorrow — ${formatted}`;

  return formatted;
}

/**
 * Formats the time range for an agenda event.
 * - Single-day:     "9:00 AM – 10:30 AM"
 * - Multi-day start: "9:00 AM – 10:30 AM (continues)"
 * - Multi-day mid:  "(continued) — all day — (continues)"
 * - Multi-day end:  "(continued) – 10:30 AM"
 */
export function formatAgendaEventTime(event: CalendarEventDto, day: Date): string {
  const eventStart = new Date(event.startTime);
  const eventEnd = new Date(event.endTime);
  const dayStart = getDayStart(day);
  const dayEnd = getDayEnd(day);

  const startsBeforeDay = eventStart < dayStart;
  const endsAfterDay = eventEnd > dayEnd;

  if (startsBeforeDay && endsAfterDay) {
    return '(continued) — all day — (continues)';
  }

  const displayStart = startsBeforeDay ? null : formatTimeShort(eventStart);
  const displayEnd = endsAfterDay ? null : formatTimeShort(eventEnd);

  if (startsBeforeDay && displayEnd) {
    return `(continued) – ${displayEnd}`;
  }

  if (displayStart && endsAfterDay) {
    return `${displayStart} (continues)`;
  }

  return `${displayStart} – ${displayEnd}`;
}
