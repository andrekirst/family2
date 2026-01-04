import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { familyGuard, noFamilyGuard } from './core/guards/family.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/components/login/login.component').then(m => m.LoginComponent),
    title: 'Sign In - Family Hub'
  },
  {
    path: 'auth/callback',
    loadComponent: () =>
      import('./features/auth/components/callback/callback.component').then(m => m.CallbackComponent),
    title: 'Authenticating - Family Hub'
  },
  {
    path: 'family/create',
    loadComponent: () =>
      import('./features/family/pages/family-wizard-page/family-wizard-page.component').then(m => m.FamilyWizardPageComponent),
    canActivate: [authGuard, noFamilyGuard],
    title: 'Create Your Family - Family Hub'
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard, familyGuard],
    title: 'Dashboard - Family Hub'
  },
  {
    path: '**',
    redirectTo: '/login'
  }
];
