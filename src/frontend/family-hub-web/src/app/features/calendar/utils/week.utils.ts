import { CalendarEventDto } from '../services/calendar.service';
import { WeekDay, PositionedEvent, WEEK_GRID_CONSTANTS } from '../models/calendar.models';

const DAY_LABELS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

// ── Date Calculation ────────────────────────────────────────────────

/** Returns Monday 00:00:00.000 of the ISO week containing `date`. */
export function getWeekStart(date: Date): Date {
  const d = new Date(date);
  d.setHours(0, 0, 0, 0);
  const day = d.getDay(); // 0=Sun, 1=Mon, ..., 6=Sat
  const diff = day === 0 ? 6 : day - 1; // Monday-based offset
  d.setDate(d.getDate() - diff);
  return d;
}

/** Returns Sunday 23:59:59.999 of the ISO week containing `date`. */
export function getWeekEnd(date: Date): Date {
  const start = getWeekStart(date);
  const end = new Date(start);
  end.setDate(end.getDate() + 6);
  end.setHours(23, 59, 59, 999);
  return end;
}

/** Returns 7 WeekDay objects (Mon–Sun) for the week containing `date`. */
export function getWeekDays(date: Date): WeekDay[] {
  const start = getWeekStart(date);
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const days: WeekDay[] = [];
  for (let i = 0; i < 7; i++) {
    const d = new Date(start);
    d.setDate(d.getDate() + i);
    days.push({
      date: d,
      isToday: d.getTime() === today.getTime(),
      dayLabel: DAY_LABELS[d.getDay()],
      dayNumber: d.getDate(),
    });
  }
  return days;
}

/**
 * Formats a week range label, e.g.:
 * - Same month: "Feb 10 – 16, 2026"
 * - Cross-month: "Jan 27 – Feb 2, 2026"
 * - Cross-year: "Dec 29, 2025 – Jan 4, 2026"
 */
export function formatWeekLabel(start: Date, end: Date): string {
  const startMonth = start.toLocaleDateString('en-US', { month: 'short' });
  const endMonth = end.toLocaleDateString('en-US', { month: 'short' });

  if (start.getFullYear() !== end.getFullYear()) {
    return `${startMonth} ${start.getDate()}, ${start.getFullYear()} – ${endMonth} ${end.getDate()}, ${end.getFullYear()}`;
  }
  if (start.getMonth() === end.getMonth()) {
    return `${startMonth} ${start.getDate()} – ${end.getDate()}, ${start.getFullYear()}`;
  }
  return `${startMonth} ${start.getDate()} – ${endMonth} ${end.getDate()}, ${end.getFullYear()}`;
}

// ── Time Conversion ─────────────────────────────────────────────────

/** Converts a Date's hour/minute to a pixel offset (1 px = 1 min). */
export function timeToPixelOffset(date: Date): number {
  return date.getHours() * WEEK_GRID_CONSTANTS.HOUR_HEIGHT + date.getMinutes();
}

/**
 * Converts a grid Y pixel offset to a Date on `dayDate`, snapped to 15-min intervals.
 * Clamps to [0, TOTAL_HEIGHT).
 */
export function pixelOffsetToTime(yOffset: number, dayDate: Date): Date {
  const clamped = Math.max(0, Math.min(yOffset, WEEK_GRID_CONSTANTS.TOTAL_HEIGHT - 1));
  const snapped =
    Math.round(clamped / WEEK_GRID_CONSTANTS.SNAP_MINUTES) * WEEK_GRID_CONSTANTS.SNAP_MINUTES;
  const totalMinutes = Math.min(snapped, 23 * 60 + 45); // Cap at 23:45
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  const result = new Date(dayDate);
  result.setHours(hours, minutes, 0, 0);
  return result;
}

/** Formats a Date to short time, e.g. "9:00 AM", "2:30 PM". */
export function formatTimeShort(date: Date): string {
  return date.toLocaleTimeString('en-US', {
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  });
}

// ── Event Filtering & Layout ────────────────────────────────────────

/** Returns events that overlap with `day` (handles multi-day events). */
export function getEventsForDay(events: CalendarEventDto[], day: Date): CalendarEventDto[] {
  const dayStart = new Date(day);
  dayStart.setHours(0, 0, 0, 0);
  const dayEnd = new Date(day);
  dayEnd.setHours(23, 59, 59, 999);

  return events.filter((e) => {
    const start = new Date(e.startTime);
    const end = new Date(e.endTime);
    return start <= dayEnd && end >= dayStart;
  });
}

/** Separates events into all-day and timed buckets. */
export function partitionEvents(
  events: CalendarEventDto[],
  day: Date,
): { allDay: CalendarEventDto[]; timed: CalendarEventDto[] } {
  const dayEvents = getEventsForDay(events, day);
  const allDay: CalendarEventDto[] = [];
  const timed: CalendarEventDto[] = [];

  for (const e of dayEvents) {
    if (e.isAllDay) {
      allDay.push(e);
    } else {
      timed.push(e);
    }
  }

  return { allDay, timed };
}

/**
 * Core overlap layout algorithm (greedy column-packing).
 *
 * 1. Sort by start asc, then duration desc
 * 2. Group overlapping events into clusters
 * 3. Greedily assign columns within each cluster
 * 4. Return PositionedEvent[] with pixel coordinates
 */
export function layoutTimedEvents(events: CalendarEventDto[], dayDate: Date): PositionedEvent[] {
  if (events.length === 0) return [];

  const dayStart = new Date(dayDate);
  dayStart.setHours(0, 0, 0, 0);
  const dayEnd = new Date(dayDate);
  dayEnd.setHours(23, 59, 59, 999);

  // Build sortable event entries with clamped start/end
  const entries = events.map((event) => {
    const rawStart = new Date(event.startTime);
    const rawEnd = new Date(event.endTime);
    const clampedStart = rawStart < dayStart ? dayStart : rawStart;
    const clampedEnd = rawEnd > dayEnd ? dayEnd : rawEnd;

    const startMinutes = clampedStart.getHours() * 60 + clampedStart.getMinutes();
    const endMinutes = clampedEnd.getHours() * 60 + clampedEnd.getMinutes();
    const duration = Math.max(endMinutes - startMinutes, WEEK_GRID_CONSTANTS.MIN_EVENT_HEIGHT);

    return { event, startMinutes, endMinutes: startMinutes + duration };
  });

  // Sort: start asc, then longer events first
  entries.sort(
    (a, b) =>
      a.startMinutes - b.startMinutes ||
      b.endMinutes - b.startMinutes - (a.endMinutes - a.startMinutes),
  );

  // Group overlapping events into clusters
  const clusters: (typeof entries)[] = [];
  let currentCluster = [entries[0]];
  let clusterEnd = entries[0].endMinutes;

  for (let i = 1; i < entries.length; i++) {
    if (entries[i].startMinutes < clusterEnd) {
      // Overlaps with current cluster
      currentCluster.push(entries[i]);
      clusterEnd = Math.max(clusterEnd, entries[i].endMinutes);
    } else {
      clusters.push(currentCluster);
      currentCluster = [entries[i]];
      clusterEnd = entries[i].endMinutes;
    }
  }
  clusters.push(currentCluster);

  // Assign columns within each cluster
  const result: PositionedEvent[] = [];

  for (const cluster of clusters) {
    // columns[col] = end time of last event in that column
    const columns: number[] = [];

    const assignments: { entry: (typeof entries)[0]; column: number }[] = [];

    for (const entry of cluster) {
      // Find first column where last event ends before this starts
      let placed = false;
      for (let col = 0; col < columns.length; col++) {
        if (columns[col] <= entry.startMinutes) {
          columns[col] = entry.endMinutes;
          assignments.push({ entry, column: col });
          placed = true;
          break;
        }
      }
      if (!placed) {
        assignments.push({ entry, column: columns.length });
        columns.push(entry.endMinutes);
      }
    }

    const totalColumns = columns.length;

    for (const { entry, column } of assignments) {
      result.push({
        event: entry.event,
        top: entry.startMinutes,
        height: entry.endMinutes - entry.startMinutes,
        column,
        totalColumns,
      });
    }
  }

  return result;
}

/** Returns the now-indicator pixel offset (minutes since midnight). */
export function getNowIndicatorOffset(): number {
  const now = new Date();
  return now.getHours() * 60 + now.getMinutes();
}
