import { CalendarEventDto } from '../services/calendar.service';
import {
  getWeekStart,
  getWeekEnd,
  getWeekDays,
  formatWeekLabel,
  timeToPixelOffset,
  pixelOffsetToTime,
  formatTimeShort,
  getEventsForDay,
  partitionEvents,
  layoutTimedEvents,
  getNowIndicatorOffset,
} from './week.utils';

// ── Helpers ─────────────────────────────────────────────────────────

function makeEvent(overrides: Partial<CalendarEventDto> = {}): CalendarEventDto {
  return {
    id: '1',
    familyId: 'f1',
    createdBy: 'u1',
    title: 'Test Event',
    description: null,
    location: null,
    startTime: '2026-02-11T09:00:00.000Z',
    endTime: '2026-02-11T10:00:00.000Z',
    isAllDay: false,

    isCancelled: false,
    createdAt: '2026-01-01T00:00:00.000Z',
    updatedAt: '2026-01-01T00:00:00.000Z',
    attendees: [],
    ...overrides,
  };
}

// ── getWeekStart ────────────────────────────────────────────────────

describe('getWeekStart', () => {
  it('returns Monday for a Wednesday input', () => {
    const wed = new Date(2026, 1, 11); // Wed Feb 11
    const result = getWeekStart(wed);
    expect(result.getDay()).toBe(1); // Monday
    expect(result.getDate()).toBe(9);
  });

  it('returns same day for Monday input', () => {
    const mon = new Date(2026, 1, 9); // Mon Feb 9
    const result = getWeekStart(mon);
    expect(result.getDay()).toBe(1);
    expect(result.getDate()).toBe(9);
  });

  it('returns previous Monday for Sunday input', () => {
    const sun = new Date(2026, 1, 15); // Sun Feb 15
    const result = getWeekStart(sun);
    expect(result.getDay()).toBe(1);
    expect(result.getDate()).toBe(9);
  });

  it('handles month boundary (Sunday in new month)', () => {
    const sun = new Date(2026, 2, 1); // Sun Mar 1
    const result = getWeekStart(sun);
    expect(result.getDay()).toBe(1);
    expect(result.getMonth()).toBe(1); // February
    expect(result.getDate()).toBe(23);
  });

  it('handles year boundary', () => {
    const thu = new Date(2026, 0, 1); // Thu Jan 1, 2026
    const result = getWeekStart(thu);
    expect(result.getDay()).toBe(1);
    expect(result.getFullYear()).toBe(2025);
    expect(result.getMonth()).toBe(11); // December
    expect(result.getDate()).toBe(29);
  });

  it('zeroes out time components', () => {
    const dateWithTime = new Date(2026, 1, 11, 14, 30, 45, 123);
    const result = getWeekStart(dateWithTime);
    expect(result.getHours()).toBe(0);
    expect(result.getMinutes()).toBe(0);
    expect(result.getSeconds()).toBe(0);
    expect(result.getMilliseconds()).toBe(0);
  });
});

// ── getWeekEnd ──────────────────────────────────────────────────────

describe('getWeekEnd', () => {
  it('returns Sunday 23:59:59.999', () => {
    const wed = new Date(2026, 1, 11);
    const result = getWeekEnd(wed);
    expect(result.getDay()).toBe(0); // Sunday
    expect(result.getDate()).toBe(15);
    expect(result.getHours()).toBe(23);
    expect(result.getMinutes()).toBe(59);
    expect(result.getSeconds()).toBe(59);
    expect(result.getMilliseconds()).toBe(999);
  });

  it('returns same Sunday for Sunday input', () => {
    const sun = new Date(2026, 1, 15);
    const result = getWeekEnd(sun);
    expect(result.getDate()).toBe(15);
  });
});

// ── getWeekDays ─────────────────────────────────────────────────────

describe('getWeekDays', () => {
  it('returns 7 days', () => {
    const days = getWeekDays(new Date(2026, 1, 11));
    expect(days).toHaveLength(7);
  });

  it('starts on Monday and ends on Sunday', () => {
    const days = getWeekDays(new Date(2026, 1, 11));
    expect(days[0].dayLabel).toBe('Mon');
    expect(days[6].dayLabel).toBe('Sun');
  });

  it('has correct day numbers', () => {
    const days = getWeekDays(new Date(2026, 1, 11));
    expect(days[0].dayNumber).toBe(9);
    expect(days[6].dayNumber).toBe(15);
  });

  it('marks today correctly', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 11, 14, 0));
    const days = getWeekDays(new Date(2026, 1, 11));
    const todayCount = days.filter((d) => d.isToday).length;
    expect(todayCount).toBe(1);
    expect(days[2].isToday).toBe(true); // Wednesday Feb 11
    vi.useRealTimers();
  });

  it('handles cross-month weeks', () => {
    const days = getWeekDays(new Date(2026, 2, 1)); // Sun Mar 1
    // Week starts Mon Feb 23
    expect(days[0].dayNumber).toBe(23);
    expect(days[0].date.getMonth()).toBe(1); // Feb
    expect(days[6].date.getMonth()).toBe(2); // Mar
  });
});

// ── formatWeekLabel ─────────────────────────────────────────────────

describe('formatWeekLabel', () => {
  it('formats same-month range', () => {
    const start = new Date(2026, 1, 9);
    const end = new Date(2026, 1, 15);
    expect(formatWeekLabel(start, end)).toBe('Feb 9 – 15, 2026');
  });

  it('formats cross-month range', () => {
    const start = new Date(2026, 0, 26);
    const end = new Date(2026, 1, 1);
    expect(formatWeekLabel(start, end)).toBe('Jan 26 – Feb 1, 2026');
  });

  it('formats cross-year range', () => {
    const start = new Date(2025, 11, 29);
    const end = new Date(2026, 0, 4);
    expect(formatWeekLabel(start, end)).toBe('Dec 29, 2025 – Jan 4, 2026');
  });
});

// ── timeToPixelOffset ───────────────────────────────────────────────

describe('timeToPixelOffset', () => {
  it('returns 0 for midnight', () => {
    const midnight = new Date(2026, 1, 11, 0, 0);
    expect(timeToPixelOffset(midnight)).toBe(0);
  });

  it('returns 540 for 9:00 AM', () => {
    const nineAm = new Date(2026, 1, 11, 9, 0);
    expect(timeToPixelOffset(nineAm)).toBe(540);
  });

  it('returns 810 for 1:30 PM', () => {
    const oneThirty = new Date(2026, 1, 11, 13, 30);
    expect(timeToPixelOffset(oneThirty)).toBe(810);
  });

  it('returns 1439 for 11:59 PM', () => {
    const lateNight = new Date(2026, 1, 11, 23, 59);
    expect(timeToPixelOffset(lateNight)).toBe(1439);
  });
});

// ── pixelOffsetToTime ───────────────────────────────────────────────

describe('pixelOffsetToTime', () => {
  const day = new Date(2026, 1, 11);

  it('snaps to 15-min intervals', () => {
    const result = pixelOffsetToTime(547, day); // ~9:07 → 9:00
    expect(result.getHours()).toBe(9);
    expect(result.getMinutes()).toBe(0);
  });

  it('snaps up at midpoint', () => {
    const result = pixelOffsetToTime(548, day); // ~9:08 → 9:15
    expect(result.getHours()).toBe(9);
    expect(result.getMinutes()).toBe(15);
  });

  it('clamps negative offset to 0:00', () => {
    const result = pixelOffsetToTime(-100, day);
    expect(result.getHours()).toBe(0);
    expect(result.getMinutes()).toBe(0);
  });

  it('clamps offset beyond 1440 to 23:45', () => {
    const result = pixelOffsetToTime(2000, day);
    expect(result.getHours()).toBe(23);
    expect(result.getMinutes()).toBe(45);
  });

  it('preserves the day date', () => {
    const result = pixelOffsetToTime(540, day);
    expect(result.getFullYear()).toBe(2026);
    expect(result.getMonth()).toBe(1);
    expect(result.getDate()).toBe(11);
  });
});

// ── formatTimeShort ─────────────────────────────────────────────────

describe('formatTimeShort', () => {
  it('formats morning time', () => {
    const result = formatTimeShort(new Date(2026, 1, 11, 9, 0));
    expect(result).toBe('9:00 AM');
  });

  it('formats afternoon time with minutes', () => {
    const result = formatTimeShort(new Date(2026, 1, 11, 14, 30));
    expect(result).toBe('2:30 PM');
  });

  it('formats midnight', () => {
    const result = formatTimeShort(new Date(2026, 1, 11, 0, 0));
    expect(result).toBe('12:00 AM');
  });
});

// ── partitionEvents ─────────────────────────────────────────────────

describe('partitionEvents', () => {
  it('separates all-day and timed events', () => {
    const events = [
      makeEvent({
        id: '1',
        isAllDay: true,
        startTime: '2026-02-11T00:00:00',
        endTime: '2026-02-11T23:59:59',
      }),
      makeEvent({
        id: '2',
        isAllDay: false,
        startTime: '2026-02-11T09:00:00',
        endTime: '2026-02-11T10:00:00',
      }),
      makeEvent({
        id: '3',
        isAllDay: true,
        startTime: '2026-02-11T00:00:00',
        endTime: '2026-02-11T23:59:59',
      }),
    ];
    const day = new Date(2026, 1, 11);
    const { allDay, timed } = partitionEvents(events, day);
    expect(allDay).toHaveLength(2);
    expect(timed).toHaveLength(1);
  });

  it('returns empty arrays for no events', () => {
    const { allDay, timed } = partitionEvents([], new Date(2026, 1, 11));
    expect(allDay).toHaveLength(0);
    expect(timed).toHaveLength(0);
  });

  it('filters events not on the given day', () => {
    const events = [
      makeEvent({ startTime: '2026-02-12T09:00:00', endTime: '2026-02-12T10:00:00' }),
    ];
    const { allDay, timed } = partitionEvents(events, new Date(2026, 1, 11));
    expect(allDay).toHaveLength(0);
    expect(timed).toHaveLength(0);
  });
});

// ── layoutTimedEvents ───────────────────────────────────────────────

describe('layoutTimedEvents', () => {
  const day = new Date(2026, 1, 11);

  it('returns empty array for no events', () => {
    expect(layoutTimedEvents([], day)).toEqual([]);
  });

  it('positions a single event correctly', () => {
    const events = [
      makeEvent({ startTime: '2026-02-11T09:00:00', endTime: '2026-02-11T10:00:00' }),
    ];
    const result = layoutTimedEvents(events, day);
    expect(result).toHaveLength(1);
    expect(result[0].top).toBe(540); // 9 * 60
    expect(result[0].height).toBe(60); // 1 hour
    expect(result[0].column).toBe(0);
    expect(result[0].totalColumns).toBe(1);
  });

  it('places non-overlapping events in same column', () => {
    const events = [
      makeEvent({ id: '1', startTime: '2026-02-11T09:00:00', endTime: '2026-02-11T10:00:00' }),
      makeEvent({ id: '2', startTime: '2026-02-11T11:00:00', endTime: '2026-02-11T12:00:00' }),
    ];
    const result = layoutTimedEvents(events, day);
    expect(result).toHaveLength(2);
    // Non-overlapping → separate clusters → each gets 1 column
    expect(result[0].totalColumns).toBe(1);
    expect(result[1].totalColumns).toBe(1);
  });

  it('places 2 overlapping events in separate columns', () => {
    const events = [
      makeEvent({ id: '1', startTime: '2026-02-11T09:00:00', endTime: '2026-02-11T10:30:00' }),
      makeEvent({ id: '2', startTime: '2026-02-11T09:30:00', endTime: '2026-02-11T11:00:00' }),
    ];
    const result = layoutTimedEvents(events, day);
    expect(result).toHaveLength(2);
    expect(result[0].column).toBe(0);
    expect(result[1].column).toBe(1);
    expect(result[0].totalColumns).toBe(2);
    expect(result[1].totalColumns).toBe(2);
  });

  it('handles 3 overlapping events', () => {
    const events = [
      makeEvent({ id: '1', startTime: '2026-02-11T09:00:00', endTime: '2026-02-11T11:00:00' }),
      makeEvent({ id: '2', startTime: '2026-02-11T09:30:00', endTime: '2026-02-11T10:30:00' }),
      makeEvent({ id: '3', startTime: '2026-02-11T10:00:00', endTime: '2026-02-11T11:30:00' }),
    ];
    const result = layoutTimedEvents(events, day);
    expect(result).toHaveLength(3);
    // All three overlap, so totalColumns should be >= 2
    const maxCol = Math.max(...result.map((r) => r.column));
    expect(maxCol).toBeGreaterThanOrEqual(1);
    expect(result[0].totalColumns).toBe(maxCol + 1);
  });

  it('handles events with same start time', () => {
    const events = [
      makeEvent({ id: '1', startTime: '2026-02-11T09:00:00', endTime: '2026-02-11T10:00:00' }),
      makeEvent({ id: '2', startTime: '2026-02-11T09:00:00', endTime: '2026-02-11T10:00:00' }),
    ];
    const result = layoutTimedEvents(events, day);
    expect(result).toHaveLength(2);
    expect(result[0].column).not.toBe(result[1].column);
    expect(result[0].totalColumns).toBe(2);
  });

  it('enforces minimum event height', () => {
    const events = [
      makeEvent({ startTime: '2026-02-11T09:00:00', endTime: '2026-02-11T09:05:00' }), // 5 min
    ];
    const result = layoutTimedEvents(events, day);
    expect(result[0].height).toBe(15); // MIN_EVENT_HEIGHT
  });

  it('clamps multi-day event to the given day', () => {
    const events = [
      makeEvent({ startTime: '2026-02-10T20:00:00', endTime: '2026-02-12T08:00:00' }),
    ];
    const result = layoutTimedEvents(events, day);
    expect(result).toHaveLength(1);
    expect(result[0].top).toBe(0); // Clamped to midnight
    expect(result[0].height).toBe(1439); // Full day (clamped end = 23:59 = 1439 min)
  });

  it('handles mixed clusters (overlap + non-overlap)', () => {
    const events = [
      makeEvent({ id: '1', startTime: '2026-02-11T09:00:00', endTime: '2026-02-11T10:00:00' }),
      makeEvent({ id: '2', startTime: '2026-02-11T09:30:00', endTime: '2026-02-11T10:30:00' }),
      makeEvent({ id: '3', startTime: '2026-02-11T14:00:00', endTime: '2026-02-11T15:00:00' }),
    ];
    const result = layoutTimedEvents(events, day);
    expect(result).toHaveLength(3);
    // First two are in a cluster with 2 columns
    const cluster1 = result.filter((r) => r.top < 660);
    expect(cluster1[0].totalColumns).toBe(2);
    // Third is in its own cluster with 1 column
    const cluster2 = result.filter((r) => r.top >= 660);
    expect(cluster2[0].totalColumns).toBe(1);
  });
});

// ── getNowIndicatorOffset ───────────────────────────────────────────

describe('getNowIndicatorOffset', () => {
  it('returns correct offset for a specific time', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 11, 14, 30)); // 2:30 PM
    expect(getNowIndicatorOffset()).toBe(870); // 14 * 60 + 30
    vi.useRealTimers();
  });

  it('returns 0 at midnight', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 11, 0, 0));
    expect(getNowIndicatorOffset()).toBe(0);
    vi.useRealTimers();
  });

  it('returns value in 0-1439 range', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 11, 23, 59));
    const offset = getNowIndicatorOffset();
    expect(offset).toBeGreaterThanOrEqual(0);
    expect(offset).toBeLessThan(1440);
    vi.useRealTimers();
  });
});

// ── getEventsForDay ─────────────────────────────────────────────────

describe('getEventsForDay', () => {
  it('includes events that span the given day', () => {
    const events = [
      makeEvent({ startTime: '2026-02-10T20:00:00', endTime: '2026-02-11T08:00:00' }),
    ];
    const result = getEventsForDay(events, new Date(2026, 1, 11));
    expect(result).toHaveLength(1);
  });

  it('excludes events on different days', () => {
    const events = [
      makeEvent({ startTime: '2026-02-12T09:00:00', endTime: '2026-02-12T10:00:00' }),
    ];
    const result = getEventsForDay(events, new Date(2026, 1, 11));
    expect(result).toHaveLength(0);
  });

  it('includes events that start at the end of the day', () => {
    const events = [
      makeEvent({ startTime: '2026-02-11T23:30:00', endTime: '2026-02-12T00:30:00' }),
    ];
    const result = getEventsForDay(events, new Date(2026, 1, 11));
    expect(result).toHaveLength(1);
  });
});
