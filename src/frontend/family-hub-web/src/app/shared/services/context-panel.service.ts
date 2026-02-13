import { Injectable, inject, signal, computed, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, NavigationStart } from '@angular/router';
import { TemplateRef } from '@angular/core';
import { filter } from 'rxjs';

export type ContextPanelMode = 'view' | 'create';

@Injectable({ providedIn: 'root' })
export class ContextPanelService {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private readonly _isOpen = signal(false);
  private readonly _template = signal<TemplateRef<unknown> | null>(null);
  private readonly _itemId = signal<string | null>(null);
  private readonly _mode = signal<ContextPanelMode>('view');

  readonly isOpen = computed(() => this._isOpen());
  readonly template = computed(() => this._template());
  readonly itemId = computed(() => this._itemId());
  readonly mode = computed(() => this._mode());

  constructor() {
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationStart),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.close());
  }

  open(template: TemplateRef<unknown>, itemId?: string): void {
    if (itemId != null) {
      // View mode: toggle if same item
      if (this._isOpen() && this._itemId() === itemId) {
        this.close();
        return;
      }
      this._mode.set('view');
      this._itemId.set(itemId);
    } else {
      // Create mode: always open fresh (no toggle for null itemId)
      this._mode.set('create');
      this._itemId.set(null);
    }
    this._template.set(template);
    this._isOpen.set(true);
  }

  setItemId(id: string): void {
    this._itemId.set(id);
    this._mode.set('view');
  }

  close(): void {
    this._isOpen.set(false);
    this._template.set(null);
    this._itemId.set(null);
    this._mode.set('view');
  }
}
