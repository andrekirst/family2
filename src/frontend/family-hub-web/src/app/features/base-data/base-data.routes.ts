import { Routes } from '@angular/router';
import { BaseDataLayoutComponent } from './components/base-data-layout/base-data-layout.component';

export const BASE_DATA_ROUTES: Routes = [
  {
    path: '',
    component: BaseDataLayoutComponent,
    children: [
      { path: '', redirectTo: 'federal-states', pathMatch: 'full' },
      {
        path: 'federal-states',
        loadComponent: () =>
          import('./components/federal-states-page/federal-states-page.component').then(
            (m) => m.FederalStatesPageComponent,
          ),
      },
    ],
  },
];
