import { NlpRule } from '../models';
import { parseRelativeDate } from './date-parser';
import { parseTime } from './time-parser';

export const DE_RULES: NlpRule[] = [
  // === Calendar event creation (existing) ===
  {
    // "morgen Termin um 10 Uhr" / "heute Termin um 14:30"
    pattern:
      /^(heute|morgen|übermorgen|montag|dienstag|mittwoch|donnerstag|freitag|samstag|sonntag)\s+(?:termin|event|veranstaltung)\s+(?:um\s+)?(.+)/i,
    extract: (match) => {
      const dateStr = match[1];
      const timeStr = match[2];
      const date = parseRelativeDate(dateStr, 'de');
      const time = parseTime(timeStr, 'de');

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
        description: `Termin erstellen${time ? ` um ${time}` : ''} am ${dateStr}`,
      };
    },
  },
  {
    // "termin morgen" / "termin am montag"
    pattern:
      /^(?:termin|event)\s+(?:am\s+)?(heute|morgen|übermorgen|montag|dienstag|mittwoch|donnerstag|freitag|samstag|sonntag)/i,
    extract: (match) => {
      const dateStr = match[1];
      const date = parseRelativeDate(dateStr, 'de');
      if (!date) return null;

      const dateParam = date.toISOString().split('T')[0];
      return {
        type: 'create-event',
        confidence: 0.65,
        date,
        route: `/family/calendar?action=create&date=${dateParam}`,
        description: `Termin erstellen am ${dateStr}`,
      };
    },
  },

  // === Navigation commands ===
  {
    pattern: /^(?:gehe\s+zu|öffne|zeige?)\s+(?:das\s+|die\s+|den\s+)?(?:dashboard|startseite)/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/dashboard',
      description: 'Zum Dashboard',
    }),
  },
  {
    pattern: /^(?:gehe\s+zu|öffne|zeige?)\s+(?:den\s+)?kalender/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/family/calendar',
      description: 'Kalender öffnen',
    }),
  },
  {
    pattern: /^(?:gehe\s+zu|öffne|zeige?)\s+(?:die\s+)?nachrichten/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/messages',
      description: 'Nachrichten öffnen',
    }),
  },
  {
    pattern: /^(?:gehe\s+zu|öffne|zeige?)\s+(?:die\s+)?dateien/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/files/browse',
      description: 'Dateien durchsuchen',
    }),
  },
  {
    pattern: /^(?:gehe\s+zu|öffne|zeige?)\s+(?:die\s+)?fotos/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/photos',
      description: 'Fotos anzeigen',
    }),
  },
  {
    pattern: /^(?:gehe\s+zu|öffne|zeige?)\s+(?:die\s+)?alben/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/files/albums',
      description: 'Alben anzeigen',
    }),
  },
  {
    pattern: /^(?:gehe\s+zu|öffne|zeige?)\s+(?:die\s+)?(?:familie|mitglieder)/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/family',
      description: 'Familie anzeigen',
    }),
  },
  {
    pattern: /^(?:gehe\s+zu|öffne|zeige?)\s+(?:die\s+)?(?:automatisierungen?|workflows?)/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/event-chains',
      description: 'Automatisierungen anzeigen',
    }),
  },
  {
    pattern: /^(?:gehe\s+zu|öffne|zeige?)\s+(?:mein\s+)?profil/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/profile',
      description: 'Profil anzeigen',
    }),
  },
  {
    pattern: /^(?:gehe\s+zu|öffne|zeige?)\s+(?:die\s+)?einstellungen/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.85,
      route: '/settings',
      description: 'Einstellungen öffnen',
    }),
  },

  // === Action commands ===
  {
    // "einlade john@example.com" / "lade ein john@example.com"
    pattern: /^(?:einladen|lade\s+ein)\s+(\S+@\S+)/i,
    extract: (match) => ({
      type: 'invite-member',
      confidence: 0.9,
      route: `/family?action=invite&email=${encodeURIComponent(match[1])}`,
      description: `${match[1]} einladen`,
    }),
  },
  {
    // "mitglied einladen" / "person einladen"
    pattern: /^(?:mitglied|person)\s+einladen/i,
    extract: () => ({
      type: 'invite-member',
      confidence: 0.7,
      route: '/family?action=invite',
      description: 'Mitglied einladen',
    }),
  },
  {
    // "erstelle ordner Urlaub" / "neuer ordner Fotos"
    pattern: /^(?:erstelle?|neuer?|neues?)\s+ordner\s+(.+)/i,
    extract: (match) => ({
      type: 'create-folder',
      confidence: 0.8,
      route: `/files/browse?action=create-folder&name=${encodeURIComponent(match[1])}`,
      description: `Ordner "${match[1]}" erstellen`,
    }),
  },
  {
    // "erstelle album Sommer 2026"
    pattern: /^(?:erstelle?|neuer?|neues?)\s+album\s+(.+)/i,
    extract: (match) => ({
      type: 'create-album',
      confidence: 0.8,
      route: `/files/albums?action=create&name=${encodeURIComponent(match[1])}`,
      description: `Album "${match[1]}" erstellen`,
    }),
  },
  {
    // "nachricht schreiben" / "schreiben"
    pattern: /^(?:nachricht\s+)?schreiben/i,
    extract: () => ({
      type: 'send-message',
      confidence: 0.75,
      route: '/messages?action=create',
      description: 'Neue Nachricht schreiben',
    }),
  },
  {
    // "suche dateien Bericht" / "finde Urlaub"
    pattern: /^(?:suche?|finde?)\s+(?:dateien?\s+)?(.+)/i,
    extract: (match) => ({
      type: 'search-files',
      confidence: 0.7,
      route: `/files/search?q=${encodeURIComponent(match[1])}`,
      description: `Dateien suchen: "${match[1]}"`,
    }),
  },
  {
    // "hochladen datei" / "upload foto"
    pattern: /^(?:hochladen|upload)\s+(?:datei|foto)?/i,
    extract: () => ({
      type: 'navigate',
      confidence: 0.75,
      route: '/files/browse?action=upload',
      description: 'Datei hochladen',
    }),
  },
];
