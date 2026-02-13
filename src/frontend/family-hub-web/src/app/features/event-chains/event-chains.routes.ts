import { Routes } from '@angular/router';

export const EVENT_CHAINS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/automations-list/automations-list.component').then(
        (m) => m.AutomationsListComponent,
      ),
  },
];
