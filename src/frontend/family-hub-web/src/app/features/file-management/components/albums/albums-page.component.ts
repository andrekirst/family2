import { Component } from '@angular/core';

@Component({
  selector: 'app-albums-page',
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center h-64 text-gray-400">
      <p class="text-lg font-medium" i18n="@@files.comingSoon">Coming soon</p>
      <p class="text-sm mt-1" i18n="@@files.albums.comingSoonDesc">
        Organize your photos into albums.
      </p>
    </div>
  `,
})
export class AlbumsPageComponent {}
