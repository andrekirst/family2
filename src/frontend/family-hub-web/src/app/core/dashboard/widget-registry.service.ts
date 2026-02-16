import { Injectable, inject } from '@angular/core';
import { DASHBOARD_WIDGET } from './dashboard.tokens';
import { WidgetRegistration } from './widget-registry.model';

@Injectable({ providedIn: 'root' })
export class WidgetRegistryService {
  private registrations: WidgetRegistration[] = [];

  constructor() {
    try {
      const injectedWidgets = inject(DASHBOARD_WIDGET, { optional: true });
      if (injectedWidgets) {
        this.registrations = injectedWidgets.flat();
      }
    } catch {
      // No widgets registered yet
    }
  }

  getAll(): WidgetRegistration[] {
    return this.registrations;
  }

  getById(id: string): WidgetRegistration | undefined {
    return this.registrations.find((w) => w.id === id);
  }

  isValid(id: string): boolean {
    return this.registrations.some((w) => w.id === id);
  }
}
