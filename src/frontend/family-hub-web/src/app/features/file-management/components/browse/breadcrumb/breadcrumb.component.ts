import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer } from '@angular/platform-browser';
import { ICONS } from '../../../../../shared/icons/icons';
import { FolderDto } from '../../../models/folder.models';

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule],
  template: `
    <nav class="flex items-center gap-1 text-sm" aria-label="Breadcrumb">
      <button
        (click)="navigated.emit(null)"
        class="text-gray-500 hover:text-gray-700 font-medium transition-colors"
        i18n="@@files.breadcrumb.home"
      >
        Home
      </button>
      @for (crumb of breadcrumbs(); track crumb.id) {
        <span [innerHTML]="chevronIcon" class="text-gray-400 flex-shrink-0"></span>
        <button
          (click)="navigated.emit(crumb.id)"
          class="text-gray-500 hover:text-gray-700 font-medium transition-colors truncate max-w-[200px]"
          [attr.title]="crumb.name"
        >
          {{ crumb.name }}
        </button>
      }
    </nav>
  `,
})
export class BreadcrumbComponent {
  readonly breadcrumbs = input<FolderDto[]>([]);
  readonly navigated = output<string | null>();

  readonly chevronIcon;

  constructor(sanitizer: DomSanitizer) {
    this.chevronIcon = sanitizer.bypassSecurityTrustHtml(ICONS.CHEVRON_RIGHT);
  }
}
