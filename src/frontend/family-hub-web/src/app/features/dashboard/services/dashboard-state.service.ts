import { Injectable, signal, computed } from '@angular/core';
import { DashboardLayoutDto, DashboardWidgetDto } from '../graphql/dashboard.operations';

@Injectable({ providedIn: 'root' })
export class DashboardStateService {
  readonly layout = signal<DashboardLayoutDto | null>(null);
  readonly isEditMode = signal(false);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);

  readonly hasWidgets = computed(() => (this.layout()?.widgets.length ?? 0) > 0);
  readonly widgetCount = computed(() => this.layout()?.widgets.length ?? 0);
  readonly dashboardName = computed(() => this.layout()?.name ?? 'My Dashboard');

  setLayout(layout: DashboardLayoutDto | null): void {
    this.layout.set(layout);
  }

  toggleEditMode(): void {
    this.isEditMode.update((v) => !v);
  }

  enterEditMode(): void {
    this.isEditMode.set(true);
  }

  exitEditMode(): void {
    this.isEditMode.set(false);
  }

  updateWidgets(widgets: DashboardWidgetDto[]): void {
    this.layout.update((l) => (l ? { ...l, widgets } : null));
  }
}
