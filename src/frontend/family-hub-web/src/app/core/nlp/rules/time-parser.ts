export function parseTime(input: string, locale: string): string | undefined {
  if (locale.startsWith('de')) {
    return parseTimeDE(input);
  }
  return parseTimeEN(input);
}

function parseTimeDE(input: string): string | undefined {
  // "um 10 Uhr", "um 14:30 Uhr", "10 Uhr", "14:30"
  const match = input.match(/(\d{1,2})(?::(\d{2}))?\s*(?:uhr)?/i);
  if (!match) return undefined;

  const hours = parseInt(match[1], 10);
  const minutes = match[2] ? parseInt(match[2], 10) : 0;

  if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59) return undefined;

  return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
}

function parseTimeEN(input: string): string | undefined {
  // "at 3 PM", "at 10:30 AM", "3pm", "10:30am"
  const match = input.match(/(\d{1,2})(?::(\d{2}))?\s*(am|pm)?/i);
  if (!match) return undefined;

  let hours = parseInt(match[1], 10);
  const minutes = match[2] ? parseInt(match[2], 10) : 0;
  const period = match[3]?.toLowerCase();

  if (period === 'pm' && hours < 12) hours += 12;
  if (period === 'am' && hours === 12) hours = 0;

  if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59) return undefined;

  return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
}
