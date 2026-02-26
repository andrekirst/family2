import { CalendarEventDto } from '../services/calendar.service';
import { AGENDA_CONSTANTS } from '../models/calendar.models';
import {
  getAgendaDateRange,
  groupEventsByDay,
  formatAgendaDayHeader,
  formatAgendaEventTime,
} from './agenda.utils';

const LOCALE_STORAGE_KEY = 'familyhub-locale';
const TIME_FORMAT_KEY = 'familyhub-time-format';

// ── Helpers ─────────────────────────────────────────────────────────

function makeEvent(overrides: Partial<CalendarEventDto> = {}): CalendarEventDto {
  return {
    id: '1',
    familyId: 'f1',
    createdBy: 'u1',
    title: 'Test Event',
    description: null,
    location: null,
    startTime: '2026-02-25T09:00:00.000Z',
    endTime: '2026-02-25T10:00:00.000Z',
    isAllDay: false,
    isCancelled: false,
    createdAt: '2026-01-01T00:00:00.000Z',
    updatedAt: '2026-01-01T00:00:00.000Z',
    attendees: [],
    ...overrides,
  };
}

// ── getAgendaDateRange ──────────────────────────────────────────────

describe('getAgendaDateRange', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 25, 10, 0)); // Feb 25, 2026
  });
  afterEach(() => vi.useRealTimers());

  it('returns start of today as start date', () => {
    const { start } = getAgendaDateRange(1);
    expect(start.getHours()).toBe(0);
    expect(start.getMinutes()).toBe(0);
    expect(start.getSeconds()).toBe(0);
    expect(start.getDate()).toBe(25);
    expect(start.getMonth()).toBe(1);
  });

  it('returns correct end date for 1 batch (30 days out)', () => {
    const { end } = getAgendaDateRange(1);
    const expected = new Date(2026, 1, 25 + AGENDA_CONSTANTS.BATCH_DAYS);
    expect(end.getDate()).toBe(expected.getDate());
    expect(end.getMonth()).toBe(expected.getMonth());
    expect(end.getHours()).toBe(23);
    expect(end.getMinutes()).toBe(59);
  });

  it('returns correct end date for 2 batches (60 days out)', () => {
    const { end } = getAgendaDateRange(2);
    const expected = new Date(2026, 1, 25 + 2 * AGENDA_CONSTANTS.BATCH_DAYS);
    expect(end.getDate()).toBe(expected.getDate());
    expect(end.getMonth()).toBe(expected.getMonth());
  });
});

// ── groupEventsByDay ────────────────────────────────────────────────

describe('groupEventsByDay', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 25, 10, 0));
    localStorage.setItem(LOCALE_STORAGE_KEY, 'en');
  });
  afterEach(() => {
    vi.useRealTimers();
    localStorage.removeItem(LOCALE_STORAGE_KEY);
  });

  it('returns empty array when no events', () => {
    const result = groupEventsByDay([], 1);
    expect(result).toEqual([]);
  });

  it('groups events by day', () => {
    const events = [
      makeEvent({
        id: '1',
        startTime: '2026-02-25T09:00:00',
        endTime: '2026-02-25T10:00:00',
      }),
      makeEvent({
        id: '2',
        startTime: '2026-02-25T14:00:00',
        endTime: '2026-02-25T15:00:00',
      }),
      makeEvent({
        id: '3',
        startTime: '2026-02-26T11:00:00',
        endTime: '2026-02-26T12:00:00',
      }),
    ];
    const result = groupEventsByDay(events, 1);
    expect(result).toHaveLength(2);
    expect(result[0].timedEvents).toHaveLength(2);
    expect(result[1].timedEvents).toHaveLength(1);
  });

  it('skips days with no events', () => {
    const events = [
      makeEvent({
        id: '1',
        startTime: '2026-02-25T09:00:00',
        endTime: '2026-02-25T10:00:00',
      }),
      makeEvent({
        id: '2',
        startTime: '2026-02-28T11:00:00',
        endTime: '2026-02-28T12:00:00',
      }),
    ];
    const result = groupEventsByDay(events, 1);
    expect(result).toHaveLength(2);
    // No groups for Feb 26 or Feb 27
    expect(result[0].date.getDate()).toBe(25);
    expect(result[1].date.getDate()).toBe(28);
  });

  it('excludes cancelled events', () => {
    const events = [
      makeEvent({
        id: '1',
        startTime: '2026-02-25T09:00:00',
        endTime: '2026-02-25T10:00:00',
        isCancelled: true,
      }),
    ];
    const result = groupEventsByDay(events, 1);
    expect(result).toHaveLength(0);
  });

  it('separates all-day and timed events', () => {
    const events = [
      makeEvent({
        id: '1',
        isAllDay: true,
        startTime: '2026-02-25T00:00:00',
        endTime: '2026-02-25T23:59:59',
      }),
      makeEvent({
        id: '2',
        startTime: '2026-02-25T09:00:00',
        endTime: '2026-02-25T10:00:00',
      }),
    ];
    const result = groupEventsByDay(events, 1);
    expect(result).toHaveLength(1);
    expect(result[0].allDayEvents).toHaveLength(1);
    expect(result[0].timedEvents).toHaveLength(1);
  });

  it('sorts timed events by start time within a day', () => {
    const events = [
      makeEvent({
        id: '1',
        startTime: '2026-02-25T14:00:00',
        endTime: '2026-02-25T15:00:00',
      }),
      makeEvent({
        id: '2',
        startTime: '2026-02-25T09:00:00',
        endTime: '2026-02-25T10:00:00',
      }),
    ];
    const result = groupEventsByDay(events, 1);
    expect(result[0].timedEvents[0].id).toBe('2'); // 9 AM first
    expect(result[0].timedEvents[1].id).toBe('1'); // 2 PM second
  });

  it('marks today correctly', () => {
    const events = [
      makeEvent({
        id: '1',
        startTime: '2026-02-25T09:00:00',
        endTime: '2026-02-25T10:00:00',
      }),
      makeEvent({
        id: '2',
        startTime: '2026-02-26T09:00:00',
        endTime: '2026-02-26T10:00:00',
      }),
    ];
    const result = groupEventsByDay(events, 1);
    expect(result[0].isToday).toBe(true);
    expect(result[1].isToday).toBe(false);
  });

  it('includes multi-day events in each spanned day', () => {
    const events = [
      makeEvent({
        id: '1',
        startTime: '2026-02-25T20:00:00',
        endTime: '2026-02-26T08:00:00',
      }),
    ];
    const result = groupEventsByDay(events, 1);
    expect(result).toHaveLength(2);
    expect(result[0].date.getDate()).toBe(25);
    expect(result[1].date.getDate()).toBe(26);
  });

  it('excludes events outside the batch range', () => {
    const events = [
      makeEvent({
        id: '1',
        startTime: '2026-02-24T09:00:00', // yesterday — before range
        endTime: '2026-02-24T10:00:00',
      }),
    ];
    const result = groupEventsByDay(events, 1);
    expect(result).toHaveLength(0);
  });
});

// ── formatAgendaDayHeader ───────────────────────────────────────────

describe('formatAgendaDayHeader', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 25, 10, 0));
    localStorage.setItem(LOCALE_STORAGE_KEY, 'en');
  });
  afterEach(() => {
    vi.useRealTimers();
    localStorage.removeItem(LOCALE_STORAGE_KEY);
  });

  it('prefixes "Today" for current date', () => {
    const result = formatAgendaDayHeader(new Date(2026, 1, 25));
    expect(result).toMatch(/^Today — /);
  });

  it('prefixes "Tomorrow" for next day', () => {
    const result = formatAgendaDayHeader(new Date(2026, 1, 26));
    expect(result).toMatch(/^Tomorrow — /);
  });

  it('shows plain date for other days', () => {
    const result = formatAgendaDayHeader(new Date(2026, 1, 28));
    expect(result).not.toContain('Today');
    expect(result).not.toContain('Tomorrow');
    expect(result).toContain('Saturday');
  });

  it('includes weekday and month in header', () => {
    const result = formatAgendaDayHeader(new Date(2026, 1, 25));
    expect(result).toContain('Wednesday');
    expect(result).toContain('Feb');
  });

  it('uses stored locale for formatting', () => {
    localStorage.setItem(LOCALE_STORAGE_KEY, 'de');
    const result = formatAgendaDayHeader(new Date(2026, 1, 28));
    expect(result).toContain('Samstag'); // German for Saturday
  });
});

// ── formatAgendaEventTime ───────────────────────────────────────────

describe('formatAgendaEventTime', () => {
  beforeEach(() => {
    localStorage.setItem(LOCALE_STORAGE_KEY, 'en');
    localStorage.setItem(TIME_FORMAT_KEY, '12h');
  });
  afterEach(() => {
    localStorage.removeItem(LOCALE_STORAGE_KEY);
    localStorage.removeItem(TIME_FORMAT_KEY);
  });

  it('formats single-day event as time range', () => {
    const event = makeEvent({
      startTime: '2026-02-25T09:00:00',
      endTime: '2026-02-25T10:30:00',
    });
    const result = formatAgendaEventTime(event, new Date(2026, 1, 25));
    expect(result).toContain('9:00 AM');
    expect(result).toContain('10:30 AM');
    expect(result).toContain('–');
  });

  it('adds (continues) for events ending after the day', () => {
    const event = makeEvent({
      startTime: '2026-02-25T20:00:00',
      endTime: '2026-02-26T08:00:00',
    });
    const result = formatAgendaEventTime(event, new Date(2026, 1, 25));
    expect(result).toContain('8:00 PM');
    expect(result).toContain('(continues)');
    expect(result).not.toContain('(continued)');
  });

  it('adds (continued) for events starting before the day', () => {
    const event = makeEvent({
      startTime: '2026-02-24T20:00:00',
      endTime: '2026-02-25T08:00:00',
    });
    const result = formatAgendaEventTime(event, new Date(2026, 1, 25));
    expect(result).toContain('(continued)');
    expect(result).toContain('8:00 AM');
    expect(result).not.toContain('(continues)');
  });

  it('shows all-day continuation for events spanning entire day', () => {
    const event = makeEvent({
      startTime: '2026-02-24T20:00:00',
      endTime: '2026-02-26T08:00:00',
    });
    const result = formatAgendaEventTime(event, new Date(2026, 1, 25));
    expect(result).toBe('(continued) — all day — (continues)');
  });

  it('respects 24h time format', () => {
    localStorage.setItem(TIME_FORMAT_KEY, '24h');
    const event = makeEvent({
      startTime: '2026-02-25T14:30:00',
      endTime: '2026-02-25T16:00:00',
    });
    const result = formatAgendaEventTime(event, new Date(2026, 1, 25));
    expect(result).toContain('14:30');
    expect(result).toContain('16:00');
  });
});
