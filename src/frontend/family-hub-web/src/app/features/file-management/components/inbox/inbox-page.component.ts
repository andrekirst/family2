import { Component } from '@angular/core';

@Component({
  selector: 'app-inbox-page',
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center h-64 text-gray-400">
      <p class="text-lg font-medium" i18n="@@files.comingSoon">Coming soon</p>
      <p class="text-sm mt-1" i18n="@@files.inbox.comingSoonDesc">Auto-organize incoming files.</p>
    </div>
  `,
})
export class InboxPageComponent {}
