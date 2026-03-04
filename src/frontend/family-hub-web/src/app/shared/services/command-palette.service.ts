import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { SearchService } from './search.service';
import { NlpParserService } from '../../core/nlp/nlp-parser.service';
import { PaletteItem } from '../models/search.models';

@Injectable({ providedIn: 'root' })
export class CommandPaletteService {
  private readonly router = inject(Router);
  private readonly searchService = inject(SearchService);
  private readonly nlpParser = inject(NlpParserService);

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
    this.items.set([]);
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
    this.close();
    this.router.navigateByUrl(item.route);
  }

  async performSearch(searchQuery: string): Promise<void> {
    if (!searchQuery.trim()) {
      this.items.set([]);
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    try {
      // Parse NLP suggestions (client-side, before GraphQL call)
      const nlpMatch = this.nlpParser.parse(searchQuery);
      const nlpItems: PaletteItem[] = nlpMatch
        ? [
            {
              type: 'nlp',
              title: nlpMatch.description,
              description: `Confidence: ${Math.round(nlpMatch.confidence * 100)}%`,
              icon: 'sparkles',
              route: nlpMatch.route,
              confidence: nlpMatch.confidence,
            },
          ]
        : [];

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
    } catch {
      this.error.set('Search failed. Please try again.');
      this.items.set([]);
    } finally {
      this.isLoading.set(false);
    }
  }
}
