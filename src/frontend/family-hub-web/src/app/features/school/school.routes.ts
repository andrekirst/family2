import { Routes } from '@angular/router';

export const SCHOOL_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/school-page/school-page.component').then((m) => m.SchoolPageComponent),
    children: [
      { path: '', redirectTo: 'students', pathMatch: 'full' },
      {
        path: 'students',
        loadComponent: () =>
          import('./components/student-list/student-list-page.component').then(
            (m) => m.StudentListPageComponent,
          ),
      },
      {
        path: 'students/:studentId',
        loadComponent: () =>
          import('./components/student-detail-page/student-detail-page.component').then(
            (m) => m.StudentDetailPageComponent,
          ),
      },
      {
        path: 'schools',
        loadComponent: () =>
          import('./components/schools-page/schools-page.component').then(
            (m) => m.SchoolsPageComponent,
          ),
      },
      {
        path: 'school-years',
        loadComponent: () =>
          import('./components/school-years-page/school-years-page.component').then(
            (m) => m.SchoolYearsPageComponent,
          ),
      },
    ],
  },
];
