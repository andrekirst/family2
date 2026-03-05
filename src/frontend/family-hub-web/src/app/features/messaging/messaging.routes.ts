import { Routes } from '@angular/router';

export const MESSAGING_ROUTES: Routes = [
  {
    path: ':conversationId',
    loadComponent: () =>
      import('./components/messaging-page/messaging-page.component').then(
        (m) => m.MessagingPageComponent,
      ),
  },
  {
    path: '',
    loadComponent: () =>
      import('./components/messaging-page/messaging-page.component').then(
        (m) => m.MessagingPageComponent,
      ),
  },
];
