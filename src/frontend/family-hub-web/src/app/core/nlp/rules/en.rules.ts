import { NlpRule } from '../models';
import { parseRelativeDate } from './date-parser';
import { parseTime } from './time-parser';

export const EN_RULES: NlpRule[] = [
  // === Calendar event creation (existing) ===
  {
    // "tomorrow event at 3 PM" / "today meeting at 10:30 AM"
    pattern:
      /^(today|tomorrow|monday|tuesday|wednesday|thursday|friday|saturday|sunday)\s+(?:event|meeting|appointment)\s+(?:at\s+)?(.+)/i,
    extract: (match) => {
      const dateStr = match[1];
      const timeStr = match[2];
      const date = parseRelativeDate(dateStr, 'en');
      const time = parseTime(timeStr, 'en');

      if (!date) return null;

      const dateParam = date.toISOString().split('T')[0];
      const route = time
        ? `/family/calendar?action=create&date=${dateParam}&time=${time}`
        : `/family/calendar?action=create&date=${dateParam}`;

      return {
        type: 'create-event',
        confidence: time ? 0.9 : 0.7,
        date,
        time,
        route,
        description: `Create event${time ? ` at ${time}` : ''} on ${dateStr}`,
      };
    },
  },
  {
    // "event tomorrow" / "meeting on friday"
    pattern:
      /^(?:event|meeting|appointment)\s+(?:on\s+)?(today|tomorrow|monday|tuesday|wednesday|thursday|friday|saturday|sunday)/i,
    extract: (match) => {
      const dateStr = match[1];
      const date = parseRelativeDate(dateStr, 'en');
      if (!date) return null;

      const dateParam = date.toISOString().split('T')[0];
      return {
        type: 'create-event',
        confidence: 0.65,
        date,
        route: `/family/calendar?action=create&date=${dateParam}`,
        description: `Create event on ${dateStr}`,
      };
    },
  },

  // === Navigation commands ===
  {
    pattern: /^(?:go\s+to|open|show|view)\s+(?:the\s+)?dashboard/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/dashboard',
      description: 'Go to Dashboard',
    }),
  },
  {
    pattern: /^(?:go\s+to|open|show|view)\s+(?:the\s+)?calendar/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/family/calendar',
      description: 'Open Calendar',
    }),
  },
  {
    pattern: /^(?:go\s+to|open|show|view)\s+(?:the\s+)?messages?/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/messages',
      description: 'Open Messages',
    }),
  },
  {
    pattern: /^(?:go\s+to|open|show|view)\s+(?:the\s+)?files?/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/files/browse',
      description: 'Browse Files',
    }),
  },
  {
    pattern: /^(?:go\s+to|open|show|view)\s+(?:the\s+)?photos?/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/photos',
      description: 'View Photos',
    }),
  },
  {
    pattern: /^(?:go\s+to|open|show|view)\s+(?:the\s+)?albums?/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/files/albums',
      description: 'View Albums',
    }),
  },
  {
    pattern: /^(?:go\s+to|open|show|view)\s+(?:the\s+)?(?:family|members?)/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/family',
      description: 'View Family',
    }),
  },
  {
    pattern: /^(?:go\s+to|open|show|view)\s+(?:the\s+)?(?:automations?|workflows?|chains?)/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/event-chains',
      description: 'View Automations',
    }),
  },
  {
    pattern: /^(?:go\s+to|open|show|view)\s+(?:my\s+)?profile/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/profile',
      description: 'View Profile',
    }),
  },
  {
    pattern: /^(?:go\s+to|open|show|view)\s+(?:the\s+)?settings/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/settings',
      description: 'Open Settings',
    }),
  },

  // === Action commands ===
  {
    // "invite john@example.com"
    pattern: /^invite\s+(\S+@\S+)/i,
    extract: (match) => ({
      type: 'invite-member',
      confidence: 0.9,
      route: `/family?action=invite&email=${encodeURIComponent(match[1])}`,
      description: `Invite ${match[1]}`,
    }),
  },
  {
    // "invite a member" / "add member"
    pattern: /^(?:invite|add)\s+(?:a\s+)?(?:member|person)/i,
    extract: () => ({
      type: 'invite-member',
      confidence: 0.7,
      route: '/family?action=invite',
      description: 'Invite a member',
    }),
  },
  {
    // "create folder Vacation" / "new folder Photos"
    pattern: /^(?:create|new)\s+folder\s+(.+)/i,
    extract: (match) => ({
      type: 'create-folder',
      confidence: 0.8,
      route: `/files/browse?action=create-folder&name=${encodeURIComponent(match[1])}`,
      description: `Create folder "${match[1]}"`,
    }),
  },
  {
    // "create album Summer 2026"
    pattern: /^(?:create|new)\s+album\s+(.+)/i,
    extract: (match) => ({
      type: 'create-album',
      confidence: 0.8,
      route: `/files/albums?action=create&name=${encodeURIComponent(match[1])}`,
      description: `Create album "${match[1]}"`,
    }),
  },
  {
    // "send a message" / "write message"
    pattern: /^(?:send|write)\s+(?:a\s+)?message/i,
    extract: () => ({
      type: 'send-message',
      confidence: 0.75,
      route: '/messages?action=create',
      description: 'Send a new message',
    }),
  },
  {
    // "find files named report" / "search vacation"
    pattern: /^(?:find|search)\s+(?:files?\s+)?(?:named?\s+)?(.+)/i,
    extract: (match) => ({
      type: 'search-files',
      confidence: 0.7,
      route: `/files/search?q=${encodeURIComponent(match[1])}`,
      description: `Search files: "${match[1]}"`,
    }),
  },
  {
    // "upload a file" / "upload photo"
    pattern: /^upload\s+(?:a\s+)?(?:file|photo)/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.75,
      route: '/files/browse?action=upload',
      description: 'Upload a file',
    }),
  },
];
