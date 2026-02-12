import { Injectable, signal } from '@angular/core';

const STORAGE_KEY = 'sidebar-collapsed';

@Injectable({ providedIn: 'root' })
export class SidebarStateService {
  readonly isCollapsed = signal(this.loadState());

  toggle(): void {
    const next = !this.isCollapsed();
    this.isCollapsed.set(next);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
  }

  private loadState(): boolean {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? JSON.parse(stored) : false;
  }
}
