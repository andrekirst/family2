import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TagDto } from '../../models/tag.models';

@Component({
  selector: 'app-tag-chip',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span
      class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium"
      [style.background-color]="tag().color + '20'"
      [style.color]="tag().color"
      [style.border]="'1px solid ' + tag().color + '40'"
    >
      {{ tag().name }}
      @if (removable()) {
        <button
          (click)="removed.emit(tag()); $event.stopPropagation()"
          class="ml-0.5 hover:opacity-70 transition-opacity"
          [attr.aria-label]="'Remove tag ' + tag().name"
        >
          &times;
        </button>
      }
    </span>
  `,
})
export class TagChipComponent {
  readonly tag = input.required<TagDto>();
  readonly removable = input(false);
  readonly removed = output<TagDto>();
}
