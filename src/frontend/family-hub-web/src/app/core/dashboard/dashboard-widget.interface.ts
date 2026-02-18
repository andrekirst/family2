import { WritableSignal } from '@angular/core';

export interface DashboardWidgetComponent {
  widgetConfig: WritableSignal<Record<string, unknown> | null>;
}
