import { Injectable, inject, signal, computed, LOCALE_ID } from '@angular/core';
import { Router } from '@angular/router';
import { SearchService } from './search.service';
import { NlpParserService } from '../../core/nlp/nlp-parser.service';
import { PaletteItem } from '../models/search.models';

@Injectable({ providedIn: 'root' })
export class CommandPaletteService {
  private readonly router = inject(Router);
  private readonly searchService = inject(SearchService);
  private readonly nlpParser = inject(NlpParserService);
  private readonly localeId = inject(LOCALE_ID);

  private readonly hintPoolEn = [
    'tomorrow event at 3 PM',
    'invite john@example.com',
    'open calendar',
    'create folder Vacation',
    'send a message',
    'event friday at 10 AM',
    'find files report',
    'go to dashboard',
    'create album Summer',
    'upload a file',
  ];

  private readonly hintPoolDe = [
    'morgen Termin um 15 Uhr',
    'john@example.com einladen',
    'Kalender öffnen',
    'Ordner Urlaub erstellen',
    'eine Nachricht senden',
    'Termin Freitag um 10 Uhr',
    'Dateien Bericht finden',
    'zum Dashboard gehen',
    'Album Sommer erstellen',
    'eine Datei hochladen',
  ];

  readonly isOpen = signal(false);
  readonly query = signal('');
  readonly selectedIndex = signal(0);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly items = signal<PaletteItem[]>([]);

  readonly hasItems = computed(() => this.items().length > 0);

  open(): void {
    this.isOpen.set(true);
    this.query.set('');
    this.selectedIndex.set(0);
    this.items.set(this.getDefaultItems());
    this.error.set(null);
  }

  close(): void {
    this.isOpen.set(false);
    this.query.set('');
    this.selectedIndex.set(0);
    this.items.set([]);
    this.error.set(null);
  }

  toggle(): void {
    if (this.isOpen()) {
      this.close();
    } else {
      this.open();
    }
  }

  moveSelection(direction: 'up' | 'down'): void {
    const total = this.items().length;
    if (total === 0) return;

    const current = this.selectedIndex();
    if (direction === 'down') {
      this.selectedIndex.set((current + 1) % total);
    } else {
      this.selectedIndex.set((current - 1 + total) % total);
    }
  }

  executeSelected(): void {
    const currentItems = this.items();
    const index = this.selectedIndex();
    if (index >= 0 && index < currentItems.length) {
      this.executeItem(currentItems[index]);
    }
  }

  executeItem(item: PaletteItem): void {
    if (item.type === 'hint') {
      this.query.set(item.title);
      this.performSearch(item.title);
      return;
    }
    this.close();
    this.router.navigateByUrl(item.route);
  }

  async performSearch(searchQuery: string): Promise<void> {
    if (!searchQuery.trim()) {
      this.items.set(this.getDefaultItems());
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    try {
      // Parse NLP suggestions (client-side, independent of GraphQL)
      let nlpItems: PaletteItem[] = [];
      try {
        const nlpMatch = this.nlpParser.parse(searchQuery);
        if (nlpMatch) {
          nlpItems = [
            {
              type: 'nlp',
              title: nlpMatch.description,
              description: `Confidence: ${Math.round(nlpMatch.confidence * 100)}%`,
              icon: 'sparkles',
              route: nlpMatch.route,
              confidence: nlpMatch.confidence,
            },
          ];
        }
      } catch (nlpError) {
        console.error('NLP parsing failed:', nlpError);
      }

      const result = await this.searchService.search(searchQuery);

      const paletteItems: PaletteItem[] = [
        ...nlpItems,
        ...result.results.map(
          (r) =>
            ({
              type: 'result',
              title: r.title,
              description: r.description,
              icon: r.icon,
              route: r.route,
              module: r.module,
            }) as PaletteItem,
        ),
        ...result.commands.map(
          (c) =>
            ({
              type: 'command',
              title: c.label,
              description: c.description,
              icon: c.icon,
              route: c.route,
              module: c.group,
            }) as PaletteItem,
        ),
      ];

      this.items.set(paletteItems);
      this.selectedIndex.set(0);
    } catch (error) {
      console.error('Command palette search failed:', error);
      this.error.set('Search failed. Please try again.');
      this.items.set([]);
    } finally {
      this.isLoading.set(false);
    }
  }

  private getDailyIndex(poolSize: number, offset: number = 0): number {
    const daysSinceEpoch = Math.floor(Date.now() / 86_400_000);
    return (daysSinceEpoch + offset) % poolSize;
  }

  private getDefaultItems(): PaletteItem[] {
    const pool = this.localeId.startsWith('de') ? this.hintPoolDe : this.hintPoolEn;
    const hint1 = pool[this.getDailyIndex(pool.length, 0)];
    const hint2 = pool[this.getDailyIndex(pool.length, 1)];

    return [
      { type: 'hint', title: hint1, icon: 'lightbulb', route: '' },
      { type: 'hint', title: hint2, icon: 'lightbulb', route: '' },
      {
        type: 'command',
        title: 'Create Event',
        description: 'Add a new calendar event',
        icon: 'plus',
        route: '/family/calendar?action=create',
        module: 'calendar',
      },
      {
        type: 'command',
        title: 'Send Message',
        description: 'Start a new conversation',
        icon: 'chat',
        route: '/messages?action=create',
        module: 'messages',
      },
      {
        type: 'navigation',
        title: 'Dashboard',
        icon: 'home',
        route: '/dashboard',
        module: 'dashboard',
      },
      {
        type: 'navigation',
        title: 'Calendar',
        icon: 'calendar',
        route: '/family/calendar',
        module: 'calendar',
      },
      {
        type: 'navigation',
        title: 'Messages',
        icon: 'chat',
        route: '/messages',
        module: 'messages',
      },
      {
        type: 'navigation',
        title: 'Files',
        icon: 'folder',
        route: '/files/browse',
        module: 'files',
      },
    ];
  }
}
