import { Routes } from '@angular/router';

export const SCHOOL_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/school-page/school-page.component').then((m) => m.SchoolPageComponent),
  },
];
