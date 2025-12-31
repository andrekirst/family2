import { Component, Input, Output, EventEmitter } from '@angular/core';


@Component({
    selector: 'app-button',
    imports: [],
    template: `
    <button
      [type]="type"
      [disabled]="disabled || loading"
      [class]="buttonClasses"
      (click)="handleClick($event)"
      >
      @if (loading) {
        <span class="animate-spin mr-2">⟳</span>
      }
      <ng-content></ng-content>
    </button>
    `,
    styles: [`
    :host {
      display: inline-block;
    }
  `]
})
export class ButtonComponent {
  @Input() variant: 'primary' | 'secondary' | 'tertiary' = 'primary';
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() disabled = false;
  @Input() loading = false;
  @Output() clicked = new EventEmitter<Event>();

  get buttonClasses(): string {
    const baseClasses = 'inline-flex items-center justify-center font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2';

    const variantClasses = {
      primary: 'bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500 disabled:bg-gray-300',
      secondary: 'bg-blue-50 text-blue-700 border border-blue-200 hover:bg-blue-100 focus:ring-blue-500 disabled:bg-gray-100',
      tertiary: 'bg-transparent text-gray-700 hover:bg-gray-100 focus:ring-gray-500 disabled:bg-gray-50'
    };

    const sizeClasses = {
      sm: 'px-3 py-1.5 text-sm',
      md: 'px-4 py-2 text-base',
      lg: 'px-6 py-3 text-lg'
    };

    return `${baseClasses} ${variantClasses[this.variant]} ${sizeClasses[this.size]}`;
  }

  handleClick(event: Event): void {
    if (!this.disabled && !this.loading) {
      this.clicked.emit(event);
    }
  }
}
