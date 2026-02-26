import { CalendarEventDto } from '../services/calendar.service';

export type CalendarViewMode = 'month' | 'week' | 'day' | 'agenda';

export interface WeekDay {
  date: Date;
  isToday: boolean;
  dayLabel: string; // "Mon", "Tue", ...
  dayNumber: number; // 10, 11, ...
}

export interface TimeRange {
  start: Date;
  end: Date;
}

export interface PositionedEvent {
  event: CalendarEventDto;
  top: number; // px from midnight
  height: number; // px based on duration
  column: number; // 0-based column in overlap group
  totalColumns: number; // total columns in overlap group
}

export interface AgendaDayGroup {
  date: Date;
  label: string; // "Today — Thursday, Feb 12" / "Tomorrow — Friday, Feb 13" / plain date
  isToday: boolean;
  allDayEvents: CalendarEventDto[];
  timedEvents: CalendarEventDto[];
}

export const WEEK_GRID_CONSTANTS = {
  HOUR_HEIGHT: 60, // px per hour
  TOTAL_HOURS: 24,
  TOTAL_HEIGHT: 1440, // 24 * 60
  SNAP_MINUTES: 15, // click-to-create snap
  DEFAULT_SCROLL_HOUR: 7, // auto-scroll target
  MIN_EVENT_HEIGHT: 15, // minimum 15px (= 15 min)
  NOW_INDICATOR_INTERVAL: 60_000,
} as const;

export const AGENDA_CONSTANTS = {
  BATCH_DAYS: 30, // days per batch
  MAX_BATCHES: 12, // maximum 12 batches = ~1 year
} as const;
