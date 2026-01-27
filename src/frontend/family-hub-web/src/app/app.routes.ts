import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { familyGuard, noFamilyGuard } from './core/guards/family.guard';
import { profileSetupGuard, noProfileSetupGuard } from './core/guards/profile-setup.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/components/login/login.component').then((m) => m.LoginComponent),
    title: 'Sign In - Family Hub',
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/components/register/register.component').then(
        (m) => m.RegisterComponent
      ),
    title: 'Create Account - Family Hub',
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/components/forgot-password/forgot-password.component').then(
        (m) => m.ForgotPasswordComponent
      ),
    title: 'Reset Password - Family Hub',
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./features/auth/components/reset-password/reset-password.component').then(
        (m) => m.ResetPasswordComponent
      ),
    title: 'Set New Password - Family Hub',
  },
  {
    path: 'verify-email',
    loadComponent: () =>
      import('./features/auth/components/verify-email/verify-email.component').then(
        (m) => m.VerifyEmailComponent
      ),
    title: 'Verify Email - Family Hub',
  },
  // Profile setup (first-login) - must complete before accessing app
  {
    path: 'profile/setup',
    loadComponent: () =>
      import('./features/profile/components/profile-setup-wizard/profile-setup-wizard.component').then(
        (m) => m.ProfileSetupWizardComponent
      ),
    canActivate: [authGuard, noProfileSetupGuard],
    title: 'Complete Your Profile - Family Hub',
  },
  // Profile page (edit existing profile)
  {
    path: 'profile',
    loadComponent: () =>
      import('./features/profile/pages/profile-page/profile-page.component').then(
        (m) => m.ProfilePageComponent
      ),
    canActivate: [authGuard, profileSetupGuard],
    title: 'Profile Settings - Family Hub',
  },
  {
    path: 'family/create',
    loadComponent: () =>
      import('./features/family/pages/family-wizard-page/family-wizard-page.component').then(
        (m) => m.FamilyWizardPageComponent
      ),
    canActivate: [authGuard, profileSetupGuard, noFamilyGuard],
    title: 'Create Your Family - Family Hub',
  },
  {
    path: 'family/manage',
    loadComponent: () =>
      import('./features/family/pages/family-management/family-management.component').then(
        (m) => m.FamilyManagementComponent
      ),
    canActivate: [authGuard, profileSetupGuard, familyGuard],
    title: 'Manage Family - Family Hub',
  },
  {
    path: 'accept-invitation',
    loadComponent: () =>
      import('./features/auth/components/accept-invitation/accept-invitation.component').then(
        (m) => m.AcceptInvitationComponent
      ),
    title: 'Accept Invitation - Family Hub',
    // NO authGuard - public route for unauthenticated users
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
    canActivate: [authGuard, profileSetupGuard, familyGuard],
    title: 'Dashboard - Family Hub',
  },
  {
    path: '**',
    redirectTo: '/login',
  },
];
