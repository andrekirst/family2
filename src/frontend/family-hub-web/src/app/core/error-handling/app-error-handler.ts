import { ErrorHandler, Injectable, signal } from '@angular/core';

export interface AppError {
  message: string;
  timestamp: Date;
  originalError: unknown;
}

@Injectable()
export class AppErrorHandler extends ErrorHandler {
  private readonly _lastError = signal<AppError | null>(null);
  readonly lastError = this._lastError.asReadonly();

  private readonly _errorCount = signal(0);
  readonly errorCount = this._errorCount.asReadonly();

  override handleError(error: unknown): void {
    const appError: AppError = {
      message: this.extractMessage(error),
      timestamp: new Date(),
      originalError: error,
    };

    this._lastError.set(appError);
    this._errorCount.update((count) => count + 1);

    console.error('[AppErrorHandler] Unhandled error:', error);

    // Call the default Angular error handler for stack trace output
    super.handleError(error);
  }

  clearError(): void {
    this._lastError.set(null);
  }

  private extractMessage(error: unknown): string {
    if (error instanceof Error) {
      return error.message;
    }
    if (typeof error === 'string') {
      return error;
    }
    return 'An unexpected error occurred';
  }
}
