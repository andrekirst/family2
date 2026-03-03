import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TopBarService } from '../../../../shared/services/top-bar.service';

interface FileTab {
  path: string;
  label: string;
}

@Component({
  selector: 'app-files-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="flex flex-col h-full">
      <!-- Sub-navigation tabs -->
      <nav class="border-b border-gray-200 bg-white px-4">
        <div class="flex gap-6 -mb-px">
          @for (tab of tabs; track tab.path) {
            <a
              [routerLink]="tab.path"
              routerLinkActive="border-blue-500 text-blue-600"
              [routerLinkActiveOptions]="{ exact: tab.path === 'browse' }"
              class="py-3 px-1 text-sm font-medium border-b-2 border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 transition-colors whitespace-nowrap"
              [attr.data-testid]="'files-tab-' + tab.path"
            >
              {{ tab.label }}
            </a>
          }
        </div>
      </nav>

      <!-- Page content -->
      <div class="flex-1 overflow-auto">
        <router-outlet />
      </div>
    </div>
  `,
})
export class FilesPageComponent implements OnInit, OnDestroy {
  private readonly topBarService = inject(TopBarService);

  readonly tabs: FileTab[] = [
    { path: 'browse', label: $localize`:@@files.tab.browse:Browse` },
    { path: 'albums', label: $localize`:@@files.tab.albums:Albums` },
    { path: 'photos', label: $localize`:@@files.tab.photos:Photos` },
    { path: 'search', label: $localize`:@@files.tab.search:Search` },
    { path: 'sharing', label: $localize`:@@files.tab.sharing:Sharing` },
    { path: 'inbox', label: $localize`:@@files.tab.inbox:Inbox` },
    { path: 'notes', label: $localize`:@@files.tab.notes:Notes` },
  ];

  ngOnInit(): void {
    this.topBarService.setTitle($localize`:@@files.title:Files`);
  }

  ngOnDestroy(): void {
    this.topBarService.setTitle('');
  }
}
