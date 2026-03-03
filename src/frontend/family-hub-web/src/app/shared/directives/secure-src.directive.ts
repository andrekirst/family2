import { Directive, ElementRef, Input, OnChanges, OnDestroy, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subscription } from 'rxjs';

/**
 * Loads images through Angular's HttpClient so the auth interceptor
 * can attach the Bearer token. Native <img src="..."> bypasses
 * interceptors, causing 401s on protected endpoints.
 *
 * Usage: <img [appSecureSrc]="url" alt="..." />
 */
@Directive({
  selector: 'img[appSecureSrc]',
  standalone: true,
})
export class SecureSrcDirective implements OnChanges, OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly el: ElementRef<HTMLImageElement> = inject(ElementRef);

  @Input({ required: true, alias: 'appSecureSrc' }) url!: string;

  private objectUrl: string | null = null;
  private sub?: Subscription;

  ngOnChanges(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.cleanup();
  }

  private load(): void {
    this.cleanup();
    if (!this.url) return;

    this.sub = this.http.get(this.url, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        this.objectUrl = URL.createObjectURL(blob);
        this.el.nativeElement.src = this.objectUrl;
      },
    });
  }

  private cleanup(): void {
    this.sub?.unsubscribe();
    if (this.objectUrl) {
      URL.revokeObjectURL(this.objectUrl);
      this.objectUrl = null;
    }
  }
}
