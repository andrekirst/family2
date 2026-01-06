import { Injectable } from '@angular/core';

/**
 * Service that provides access to the native window object.
 *
 * This service exists to make window-dependent code testable by allowing
 * the window object to be mocked in unit tests. Without this wrapper,
 * window.location and other window properties cannot be easily mocked
 * due to browser security restrictions.
 *
 * @example
 * ```typescript
 * constructor(private windowRef: WindowRef) {}
 *
 * redirect(url: string): void {
 *   this.windowRef.nativeWindow.location.href = url;
 * }
 * ```
 *
 * @example Testing
 * ```typescript
 * const mockWindow = { location: { href: '' } };
 * const windowRefMock = { nativeWindow: mockWindow };
 *
 * TestBed.configureTestingModule({
 *   providers: [
 *     { provide: WindowRef, useValue: windowRefMock }
 *   ]
 * });
 * ```
 */
@Injectable({
  providedIn: 'root'
})
export class WindowRef {
  /**
   * Returns the native window object.
   * In production, this is the browser's window object.
   * In tests, this can be mocked.
   */
  get nativeWindow(): Window {
    return window;
  }
}
