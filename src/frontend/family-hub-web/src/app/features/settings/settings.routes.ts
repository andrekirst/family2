import { Routes } from '@angular/router';

export const SETTINGS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/user-settings/user-settings.component').then(
        (m) => m.UserSettingsComponent,
      ),
  },
];
