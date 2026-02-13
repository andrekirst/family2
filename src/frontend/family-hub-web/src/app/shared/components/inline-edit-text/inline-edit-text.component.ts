import {
  Component,
  Input,
  Output,
  EventEmitter,
  signal,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../icons/icons';

@Component({
  selector: 'app-inline-edit-text',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (isEditing()) {
      @if (multiline) {
        <textarea
          #inputRef
          [attr.data-testid]="testId + '-input'"
          [ngModel]="editValue()"
          (ngModelChange)="editValue.set($event)"
          (blur)="onBlur()"
          (keydown.escape)="onEscape($event)"
          [disabled]="disabled"
          [placeholder]="placeholder"
          rows="3"
          class="w-full px-2 py-1 border border-blue-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500 focus:outline-none resize-none"
          [class]="inputClasses"
        ></textarea>
      } @else {
        <input
          #inputRef
          type="text"
          [attr.data-testid]="testId + '-input'"
          [ngModel]="editValue()"
          (ngModelChange)="editValue.set($event)"
          (blur)="onBlur()"
          (keydown.enter)="onEnter($event)"
          (keydown.escape)="onEscape($event)"
          [disabled]="disabled"
          [placeholder]="placeholder"
          class="w-full px-2 py-1 border border-blue-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500 focus:outline-none"
          [class]="inputClasses"
        />
      }
    } @else {
      <div
        class="group relative cursor-pointer rounded-md px-2 py-1 -mx-2 -my-1 transition-colors"
        [class.hover:bg-gray-50]="!disabled"
        [class.cursor-default]="disabled"
        (click)="startEditing()"
        [attr.data-testid]="testId"
      >
        @if (value) {
          <span [class]="displayClasses">{{ value }}</span>
        } @else {
          <span class="text-gray-400 italic" [class]="displayClasses">{{ placeholder }}</span>
        }
        @if (!disabled) {
          <span
            class="absolute right-1 top-1/2 -translate-y-1/2 opacity-0 group-hover:opacity-100 transition-opacity text-gray-400"
            [innerHTML]="pencilIcon"
            [attr.data-testid]="testId + '-pencil'"
          ></span>
        }
      </div>
    }
  `,
})
export class InlineEditTextComponent {
  @Input() value: string | null = '';
  @Input() placeholder = 'Click to edit';
  @Input() multiline = false;
  @Input() disabled = false;
  @Input() testId = 'inline-edit';
  @Input() displayClasses = '';
  @Input() inputClasses = '';

  @Output() saved = new EventEmitter<string>();

  @ViewChild('inputRef') inputRef!: ElementRef<HTMLInputElement | HTMLTextAreaElement>;

  readonly isEditing = signal(false);
  readonly editValue = signal('');
  readonly pencilIcon: SafeHtml;

  private originalValue = '';

  constructor(private sanitizer: DomSanitizer) {
    this.pencilIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.PENCIL);
  }

  startEditing(): void {
    if (this.disabled) return;
    this.originalValue = this.value ?? '';
    this.editValue.set(this.originalValue);
    this.isEditing.set(true);
    setTimeout(() => this.inputRef?.nativeElement?.focus());
  }

  onBlur(): void {
    const newValue = this.editValue().trim();
    this.isEditing.set(false);
    if (newValue !== this.originalValue) {
      this.saved.emit(newValue);
    }
  }

  onEnter(event: Event): void {
    event.preventDefault();
    this.inputRef?.nativeElement?.blur();
  }

  onEscape(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.editValue.set(this.originalValue);
    this.isEditing.set(false);
  }
}
