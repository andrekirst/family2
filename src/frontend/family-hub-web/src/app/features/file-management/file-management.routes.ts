import { Routes } from '@angular/router';

export const FILE_MANAGEMENT_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/files-page/files-page.component').then((m) => m.FilesPageComponent),
    children: [
      { path: '', redirectTo: 'browse', pathMatch: 'full' },
      {
        path: 'browse',
        loadComponent: () =>
          import('./components/browse/browse-page.component').then((m) => m.BrowsePageComponent),
      },
      {
        path: 'albums',
        loadComponent: () =>
          import('./components/albums/albums-page.component').then((m) => m.AlbumsPageComponent),
      },
      {
        path: 'albums/:albumId',
        loadComponent: () =>
          import('./components/albums/album-detail.component').then((m) => m.AlbumDetailComponent),
      },
      {
        path: 'search',
        loadComponent: () =>
          import('./components/search/search-page.component').then((m) => m.SearchPageComponent),
      },
      {
        path: 'sharing',
        loadComponent: () =>
          import('./components/sharing/sharing-page.component').then((m) => m.SharingPageComponent),
      },
      {
        path: 'inbox',
        loadComponent: () =>
          import('./components/inbox/inbox-page.component').then((m) => m.InboxPageComponent),
      },
      {
        path: 'notes',
        loadComponent: () =>
          import('./components/notes/notes-page.component').then((m) => m.NotesPageComponent),
      },
      {
        path: 'photos',
        loadComponent: () =>
          import('../photos/components/photos-page/photos-page.component').then(
            (m) => m.PhotosPageComponent,
          ),
      },
    ],
  },
];
