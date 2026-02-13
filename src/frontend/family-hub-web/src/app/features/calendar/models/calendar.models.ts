import { CalendarEventDto } from '../services/calendar.service';

export type CalendarViewMode = 'month' | 'week';

export interface WeekDay {
  date: Date;
  isToday: boolean;
  dayLabel: string; // "Mon", "Tue", ...
  dayNumber: number; // 10, 11, ...
}

export interface PositionedEvent {
  event: CalendarEventDto;
  top: number; // px from midnight
  height: number; // px based on duration
  column: number; // 0-based column in overlap group
  totalColumns: number; // total columns in overlap group
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
