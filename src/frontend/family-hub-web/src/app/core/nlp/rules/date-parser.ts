export function parseRelativeDate(input: string, locale: string): Date | undefined {
  const today = new Date();
  const lower = input.toLowerCase().trim();

  if (locale.startsWith('de')) {
    return parseDateDE(lower, today);
  }
  return parseDateEN(lower, today);
}

function parseDateDE(input: string, today: Date): Date | undefined {
  if (input === 'heute') return today;
  if (input === 'morgen') return addDays(today, 1);
  if (input === 'übermorgen') return addDays(today, 2);

  const dayMap: Record<string, number> = {
    montag: 1,
    dienstag: 2,
    mittwoch: 3,
    donnerstag: 4,
    freitag: 5,
    samstag: 6,
    sonntag: 0,
  };

  const day = dayMap[input];
  if (day !== undefined) return nextWeekday(today, day);

  return undefined;
}

function parseDateEN(input: string, today: Date): Date | undefined {
  if (input === 'today') return today;
  if (input === 'tomorrow') return addDays(today, 1);

  const dayMap: Record<string, number> = {
    monday: 1,
    tuesday: 2,
    wednesday: 3,
    thursday: 4,
    friday: 5,
    saturday: 6,
    sunday: 0,
  };

  const day = dayMap[input];
  if (day !== undefined) return nextWeekday(today, day);

  return undefined;
}

function addDays(date: Date, days: number): Date {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}

function nextWeekday(from: Date, targetDay: number): Date {
  const result = new Date(from);
  const currentDay = result.getDay();
  let diff = targetDay - currentDay;
  if (diff <= 0) diff += 7;
  result.setDate(result.getDate() + diff);
  return result;
}
