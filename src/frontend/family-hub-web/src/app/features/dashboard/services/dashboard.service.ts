import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { map } from 'rxjs';
import {
  GET_MY_DASHBOARD,
  GET_FAMILY_DASHBOARD,
  GET_AVAILABLE_WIDGETS,
  SAVE_DASHBOARD_LAYOUT,
  ADD_WIDGET,
  REMOVE_WIDGET,
  RESET_DASHBOARD,
  DashboardLayoutDto,
  WidgetDescriptorDto,
  DashboardWidgetDto,
} from '../graphql/dashboard.operations';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private apollo = inject(Apollo);

  getMyDashboard() {
    return this.apollo
      .query<{ dashboard: { myDashboard: DashboardLayoutDto | null } }>({
        query: GET_MY_DASHBOARD,
        fetchPolicy: 'network-only',
      })
      .pipe(map((r) => r.data!.dashboard.myDashboard));
  }

  getFamilyDashboard() {
    return this.apollo
      .query<{ dashboard: { familyDashboard: DashboardLayoutDto | null } }>({
        query: GET_FAMILY_DASHBOARD,
        fetchPolicy: 'network-only',
      })
      .pipe(map((r) => r.data!.dashboard.familyDashboard));
  }

  getAvailableWidgets() {
    return this.apollo
      .query<{ dashboard: { availableWidgets: WidgetDescriptorDto[] } }>({
        query: GET_AVAILABLE_WIDGETS,
      })
      .pipe(map((r) => r.data!.dashboard.availableWidgets));
  }

  saveLayout(input: {
    name: string;
    isShared: boolean;
    widgets: {
      id?: string;
      widgetType: string;
      x: number;
      y: number;
      width: number;
      height: number;
      sortOrder: number;
      configJson?: string | null;
    }[];
  }) {
    return this.apollo
      .mutate<{ dashboard: { saveLayout: DashboardLayoutDto } }>({
        mutation: SAVE_DASHBOARD_LAYOUT,
        variables: { input },
      })
      .pipe(map((r) => r.data!.dashboard.saveLayout));
  }

  addWidget(input: {
    dashboardId: string;
    widgetType: string;
    x: number;
    y: number;
    width: number;
    height: number;
    configJson?: string | null;
  }) {
    return this.apollo
      .mutate<{ dashboard: { addWidget: DashboardWidgetDto } }>({
        mutation: ADD_WIDGET,
        variables: { input },
      })
      .pipe(map((r) => r.data!.dashboard.addWidget));
  }

  removeWidget(widgetId: string) {
    return this.apollo
      .mutate<{ dashboard: { removeWidget: boolean } }>({
        mutation: REMOVE_WIDGET,
        variables: { widgetId },
      })
      .pipe(map((r) => r.data!.dashboard.removeWidget));
  }

  resetDashboard(dashboardId: string) {
    return this.apollo
      .mutate<{ dashboard: { resetDashboard: boolean } }>({
        mutation: RESET_DASHBOARD,
        variables: { dashboardId },
      })
      .pipe(map((r) => r.data!.dashboard.resetDashboard));
  }
}
