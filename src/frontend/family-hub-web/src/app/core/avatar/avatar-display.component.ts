import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EnvironmentConfigService } from '../config/environment-config.service';

export type AvatarSizeType = 'tiny' | 'small' | 'medium' | 'large';

const SIZE_PX: Record<AvatarSizeType, number> = {
  tiny: 24,
  small: 48,
  medium: 128,
  large: 512,
};

// Deterministic color palette for initials fallback
const AVATAR_COLORS = [
  '#EF4444',
  '#F97316',
  '#F59E0B',
  '#84CC16',
  '#22C55E',
  '#14B8A6',
  '#06B6D4',
  '#3B82F6',
  '#6366F1',
  '#8B5CF6',
  '#A855F7',
  '#D946EF',
  '#EC4899',
  '#F43F5E',
];

@Component({
  selector: 'app-avatar-display',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (showImage()) {
      <img
        [src]="avatarUrl()"
        [alt]="name() + ' avatar'"
        [style.width.px]="sizePx()"
        [style.height.px]="sizePx()"
        class="rounded-full object-cover"
        (error)="onImageError()"
      />
    } @else {
      <div
        class="rounded-full flex items-center justify-center font-semibold text-white select-none"
        [style.width.px]="sizePx()"
        [style.height.px]="sizePx()"
        [style.background-color]="backgroundColor()"
        [style.font-size.px]="fontSize()"
      >
        {{ initials() }}
      </div>
    }
  `,
})
export class AvatarDisplayComponent {
  private envConfig = inject(EnvironmentConfigService);

  avatarId = input<string | null>(null);
  name = input<string>('');
  size = input<AvatarSizeType>('small');

  sizePx = computed(() => SIZE_PX[this.size()]);
  fontSize = computed(() => Math.round(this.sizePx() * 0.4));

  private imageError = signal(false);

  // Reset error state when avatarId changes so a new URL gets a fresh attempt
  private resetErrorEffect = effect(() => {
    this.avatarId(); // track dependency
    this.imageError.set(false);
  });

  avatarUrl = computed(() => {
    const id = this.avatarId();
    if (!id) return null;
    return `${this.envConfig.apiBaseUrl}/api/avatars/${id}/${this.size()}`;
  });

  showImage = computed(() => !!this.avatarUrl() && !this.imageError());

  initials = computed(() => {
    const fullName = this.name().trim();
    if (!fullName) return '?';
    const parts = fullName.split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return fullName.substring(0, 2).toUpperCase();
  });

  backgroundColor = computed(() => {
    const fullName = this.name();
    let hash = 0;
    for (let i = 0; i < fullName.length; i++) {
      hash = fullName.charCodeAt(i) + ((hash << 5) - hash);
    }
    return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length];
  });

  onImageError(): void {
    this.imageError.set(true);
  }
}
