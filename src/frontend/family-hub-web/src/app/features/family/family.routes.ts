import { Routes } from '@angular/router';

export const FAMILY_ROUTES: Routes = [
  {
    path: 'settings',
    loadComponent: () =>
      import('./components/family-settings/family-settings.component').then(
        (m) => m.FamilySettingsComponent,
      ),
  },
];
