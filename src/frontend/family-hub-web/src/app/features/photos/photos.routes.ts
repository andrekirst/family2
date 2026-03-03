import { Routes } from '@angular/router';

export const PHOTOS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/photos-page/photos-page.component').then((m) => m.PhotosPageComponent),
  },
];
