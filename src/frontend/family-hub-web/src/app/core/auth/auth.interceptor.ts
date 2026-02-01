import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { from, switchMap, catchError, throwError } from 'rxjs';

/**
 * HTTP interceptor to add Authorization header with JWT Bearer token
 * and automatically refresh expired tokens
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // Skip adding token to Keycloak token endpoint requests
  if (req.url.includes('/protocol/openid-connect/token')) {
    return next(req);
  }

  // Skip if no access token available
  const accessToken = authService.getAccessToken();
  if (!accessToken) {
    return next(req);
  }

  // If token is expired, refresh it first
  if (authService.isTokenExpired()) {
    return from(authService.refreshAccessToken()).pipe(
      switchMap((refreshed) => {
        if (!refreshed) {
          // Refresh failed - redirect to login
          authService.logout();
          return throwError(() => new Error('Authentication expired'));
        }

        // Retry request with new token
        const newToken = authService.getAccessToken();
        const clonedReq = req.clone({
          setHeaders: { Authorization: `Bearer ${newToken}` },
        });
        return next(clonedReq);
      }),
      catchError((error) => {
        authService.logout();
        return throwError(() => error);
      }),
    );
  }

  // Add Authorization header
  const clonedReq = req.clone({
    setHeaders: { Authorization: `Bearer ${accessToken}` },
  });

  return next(clonedReq);
};
