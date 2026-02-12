import { Injectable, signal, computed } from '@angular/core';

export interface TopBarAction {
  id: string;
  label: string;
  icon?: string;
  onClick: () => void;
  variant?: 'primary' | 'secondary' | 'danger';
  disabled?: boolean;
  testId?: string;
}

export interface TopBarConfig {
  title: string;
  actions?: TopBarAction[];
}

@Injectable({ providedIn: 'root' })
export class TopBarService {
  private readonly config = signal<TopBarConfig>({ title: '' });

  readonly title = computed(() => this.config().title);
  readonly actions = computed(() => this.config().actions ?? []);

  setConfig(config: TopBarConfig): void {
    this.config.set(config);
  }

  setTitle(title: string): void {
    this.config.update((c) => ({ ...c, title }));
  }

  setActions(actions: TopBarAction[]): void {
    this.config.update((c) => ({ ...c, actions }));
  }

  clear(): void {
    this.config.set({ title: '' });
  }
}
