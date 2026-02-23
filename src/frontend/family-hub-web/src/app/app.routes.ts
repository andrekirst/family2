import { Routes } from '@angular/router';
import { authGuard, familyMemberGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },

  // Public routes
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'callback',
    loadComponent: () =>
      import('./features/auth/callback/callback.component').then((m) => m.CallbackComponent),
  },
  {
    path: 'invitation/accept',
    loadComponent: () =>
      import('./features/family/components/invitation-accept/invitation-accept.component').then(
        (m) => m.InvitationAcceptComponent,
      ),
  },

  // Protected routes (group-level authGuard, wrapped in layout shell)
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./shared/layout/layout.component').then((m) => m.LayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadChildren: () =>
          import('./features/dashboard/dashboard.routes').then((m) => m.DASHBOARD_ROUTES),
      },
      {
        path: 'calendar',
        canActivate: [familyMemberGuard],
        loadChildren: () =>
          import('./features/calendar/calendar.routes').then((m) => m.CALENDAR_ROUTES),
      },
      {
        path: 'files',
        canActivate: [familyMemberGuard],
        loadChildren: () =>
          import('./features/file-management/file-management.routes').then(
            (m) => m.FILE_MANAGEMENT_ROUTES,
          ),
      },
      {
        path: 'event-chains',
        canActivate: [familyMemberGuard],
        loadChildren: () =>
          import('./features/event-chains/event-chains.routes').then((m) => m.EVENT_CHAINS_ROUTES),
      },
      {
        path: 'profile',
        loadChildren: () =>
          import('./features/profile/profile.routes').then((m) => m.PROFILE_ROUTES),
      },
      {
        path: 'family',
        loadChildren: () => import('./features/family/family.routes').then((m) => m.FAMILY_ROUTES),
      },
      {
        path: 'settings',
        loadChildren: () =>
          import('./features/settings/settings.routes').then((m) => m.SETTINGS_ROUTES),
      },
    ],
  },
];
