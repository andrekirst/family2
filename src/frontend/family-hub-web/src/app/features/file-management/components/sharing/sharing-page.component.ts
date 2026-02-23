import { Component } from '@angular/core';

@Component({
  selector: 'app-sharing-page',
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center h-64 text-gray-400">
      <p class="text-lg font-medium" i18n="@@files.comingSoon">Coming soon</p>
      <p class="text-sm mt-1" i18n="@@files.sharing.comingSoonDesc">Share files with links.</p>
    </div>
  `,
})
export class SharingPageComponent {}
