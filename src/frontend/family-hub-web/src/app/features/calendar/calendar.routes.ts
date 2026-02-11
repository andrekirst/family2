import { Routes } from '@angular/router';

export const CALENDAR_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/calendar-page/calendar-page.component').then(
        (m) => m.CalendarPageComponent,
      ),
  },
];
