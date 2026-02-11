import { CalendarEventDto } from '../services/calendar.service';

export type CalendarViewMode = 'month' | 'week';

/** Pill-style background colors for month-grid chips and week-grid badges */
export const EVENT_TYPE_COLORS: Record<string, string> = {
  Personal: 'bg-blue-100 text-blue-800 border-blue-200',
  Medical: 'bg-red-100 text-red-800 border-red-200',
  School: 'bg-yellow-100 text-yellow-800 border-yellow-200',
  Work: 'bg-purple-100 text-purple-800 border-purple-200',
  Social: 'bg-green-100 text-green-800 border-green-200',
  Travel: 'bg-orange-100 text-orange-800 border-orange-200',
  Other: 'bg-gray-100 text-gray-800 border-gray-200',
};

/** Solid left-border accent colors for week-grid timed event blocks */
export const EVENT_TYPE_BORDER_COLORS: Record<string, string> = {
  Personal: 'border-l-blue-500',
  Medical: 'border-l-red-500',
  School: 'border-l-yellow-500',
  Work: 'border-l-purple-500',
  Social: 'border-l-green-500',
  Travel: 'border-l-orange-500',
  Other: 'border-l-gray-500',
};

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
